#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Runtime;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Internal;
using ImpromptuInterface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Extensions.Scheduling.Models
{
    public class BackgroundTaskHost : IDisposable, IServerTimestampService
    {
        private static readonly IDictionary<Type, HandlerHooks> MethodCache =
            new ConcurrentDictionary<Type, HandlerHooks>();

        private readonly ConcurrentDictionary<TaskScheduler, TaskFactory> _factories;
        private readonly ConcurrentDictionary<Handler, HandlerHooks> _pending;
        private readonly ConcurrentDictionary<int, TaskScheduler> _schedulers;

        private readonly IServiceProvider _backgroundServices;
        private readonly IServerTimestampService _timestamps;
        private readonly ITypeResolver _typeResolver;

        private CancellationTokenSource _cancel;
        private QueuedTaskScheduler _scheduler;
        private PushQueue<IEnumerable<BackgroundTask>> _background;
        private PushQueue<IEnumerable<BackgroundTask>> _maintenance;

        private readonly ISafeLogger<BackgroundTaskHost> _logger;
        private readonly IOptionsMonitor<BackgroundTaskOptions> _options;

        public BackgroundTaskHost(
            IServiceProvider backgroundServices,
            IServerTimestampService timestamps,
            IBackgroundTaskStore store,
            IBackgroundTaskSerializer serializer,
            ITypeResolver typeResolver,
            IOptionsMonitor<BackgroundTaskOptions> options,
            ISafeLogger<BackgroundTaskHost> logger)
        {
            _backgroundServices = backgroundServices;
            _timestamps = timestamps;
            Store = store;
            Serializer = serializer;
            _typeResolver = typeResolver;
            _options = options;
            _logger = logger;
            options.OnChange(OnSettingsChanged);

            _schedulers = new ConcurrentDictionary<int, TaskScheduler>();
            _factories = new ConcurrentDictionary<TaskScheduler, TaskFactory>();
            _pending = new ConcurrentDictionary<Handler, HandlerHooks>();
            _cancel = new CancellationTokenSource();

            // dispatch thread
            _background = new PushQueue<IEnumerable<BackgroundTask>>();
            _background.Attach(WithPendingTasks);
            _background.AttachBacklog(WithOverflowTasks);
            _background.AttachUndeliverable(WithFailedTasks);

            // maintenance thread
            _maintenance = new PushQueue<IEnumerable<BackgroundTask>>();
            _maintenance.Attach(WithHangingTasks);
            _maintenance.AttachBacklog(WithHangingTasks);
            _maintenance.AttachUndeliverable(WithFailedTasks);
        }

        public IBackgroundTaskStore Store
        {
            get;
        }

        public IBackgroundTaskSerializer Serializer
        {
            get;
        }

        public BackgroundTaskOptions Options
        {
            get
            {
                var settings = _options.CurrentValue;

                var @readonly = new BackgroundTaskOptions
                {
                    DelayTasks = settings.DelayTasks,
                    Concurrency = settings.Concurrency,
                    SleepIntervalSeconds = settings.SleepIntervalSeconds,
                    IntervalFunction = settings.IntervalFunction,
                    ReadAhead = settings.ReadAhead,
                    MaximumAttempts = settings.MaximumAttempts,
                    MaximumRuntimeSeconds = settings.MaximumRuntimeSeconds,
                    DeleteOnError = settings.DeleteOnError,
                    DeleteOnFailure = settings.DeleteOnFailure,
                    DeleteOnSuccess = settings.DeleteOnSuccess,
                    Priority = settings.Priority
                };

                return @readonly;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnSettingsChanged(BackgroundTaskOptions changed)
        {
            _logger.Info(() => "Background task options changed, recycling the host.");
            Stop();
            Start();
        }

        private IEnumerable<BackgroundTask> EnqueueTasks()
        {
            return Store.LockNextAvailableAsync(_options.CurrentValue.ReadAhead).GetAwaiter().GetResult();
        }

        private IEnumerable<BackgroundTask> HangingTasks()
        {
            return Store.GetHangingTasksAsync().GetAwaiter().GetResult();
        }

        public void Start(bool immediate = false)
        {
            if (_scheduler == null)
            {
                _scheduler = new QueuedTaskScheduler(ResolveConcurrency());
            }

            _background.Produce(EnqueueTasks, TimeSpan.FromSeconds(_options.CurrentValue.SleepIntervalSeconds));
            _background.Start(immediate);

            _maintenance.Produce(HangingTasks, TimeSpan.FromMinutes(1));
            _maintenance.Start(immediate);
        }

        private int ResolveConcurrency()
        {
            return _options.CurrentValue.Concurrency == 0
                ? Environment.ProcessorCount
                : _options.CurrentValue.Concurrency;
        }

        public void Stop(CancellationToken cancellationToken = default, bool immediate = false)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = ResolveConcurrency()
            };

            var context = ProvisionExecutionContext(cancellationToken);

            Parallel.ForEach(_pending.Where(entry => entry.Value.OnHalt != null), options,
                async e => { await e.Value.OnHalt.HaltAsync(context, immediate); });

            _pending.Clear();

            _scheduler?.Dispose();
            _scheduler = null;

            _background.Stop(immediate);
            _maintenance.Stop(immediate);
        }

        private static ExecutionContext ProvisionExecutionContext(CancellationToken cancellationToken)
        {
            var services = new ServiceCollection();
            var context = new ExecutionContext(services.BuildServiceProvider(), new InMemoryKeyValueStore<string, object>(), cancellationToken);
            return context;
        }

        private void WithFailedTasks(IEnumerable<BackgroundTask> tasks)
        {
            // This should be impossible; we only use the pipeline to seed from a backing store, which is all or nothing
            WithPendingTasks(tasks);
        }

        private bool WithOverflowTasks(IEnumerable<BackgroundTask> tasks)
        {
            // We could have been shutting down, which is not materially different than if we had succeeded, so we should process these
            return WithPendingTasks(tasks);
        }

        private bool WithHangingTasks(IEnumerable<BackgroundTask> tasks)
        {
            var now = _timestamps.GetCurrentTime();

            foreach (var task in tasks)
            {
                // bump up attempts (wouldn't have reached here normally since we never unlocked)
                task.Attempts++;

                // unlock hanging task (record failure)
                task.LockedAt = null;
                task.LockedBy = null;
                task.LastError = ErrorStrings.ExceededRuntime;

                if (TaskIsTerminal(task))
                {
                    if (task.DeleteOnFailure.HasValue && task.DeleteOnFailure.Value)
                    {
                        Store.DeleteAsync(task);
                    }
                    else
                    {
                        task.FailedAt = now;
                    }
                }
                else
                {
                    task.RunAt += _options.CurrentValue.IntervalFunction.NextInterval(task.Attempts);
                }

                Store.SaveAsync(task);
            }

            return true;
        }

        private bool WithPendingTasks(IEnumerable<BackgroundTask> tasks)
        {
            var pendingTasks = new Dictionary<Task, CancellationTokenSource>();
            var subjects = new Dictionary<Task, BackgroundTask>();

            foreach (var task in tasks)
            {
                var scheduler = AcquireScheduler(task);
                var cancel = new CancellationTokenSource();
                var taskFactory = _factories[scheduler];

                var started = taskFactory.StartNew(async () => { await AttemptTaskAsync(task); }, cancel.Token);

                pendingTasks.Add(started, cancel);
                subjects.Add(started, task);
            }

            Parallel.ForEach(pendingTasks, new ParallelOptions { MaxDegreeOfParallelism = ResolveConcurrency() },
                performer =>
                {
                    var task = subjects[performer.Key];
                    if (Task.WaitAll(new[] { performer.Key }, task.MaximumRuntime.GetValueOrDefault()))
                    {
                        return;
                    }

                    performer.Value.Cancel();
                    task.LastError = ErrorStrings.ExceededRuntime;
                });

            return true;
        }

        internal async Task<bool> AttemptTaskAsync(BackgroundTask task, bool persist = true)
        {
            if (_cancel.IsCancellationRequested)
            {
                return false;
            }
            
            var success = await AttemptCycleAsync(task);

            if (persist)
            {
                await UpdateTaskAsync(task, success.Item1, success.Item2);
            }

            _cancel.Token.ThrowIfCancellationRequested();

            return (success.Item1);
        }

        private async Task<(bool, Exception)> AttemptCycleAsync(BackgroundTask task)
        {
            task.Attempts++;

            var success = await PerformAsync(task);
            if (!success.Item1)
            {
                task.RunAt = _timestamps.GetCurrentTime() + _options.CurrentValue.IntervalFunction.NextInterval(task.Attempts);
            }

            return success;
        }

        private async Task UpdateTaskAsync(BackgroundTask task, bool success, Exception exception)
        {
            var deleted = false;

            var now = _timestamps.GetCurrentTime();

            if (!success)
            {
                if (TaskIsTerminal(task))
                {
                    if (task.DeleteOnFailure.HasValue && task.DeleteOnFailure.Value)
                    {
                        await Store.DeleteAsync(task);
                        deleted = true;
                    }

                    task.FailedAt = now;
                }
            }
            else
            {
                if (task.DeleteOnSuccess.HasValue && task.DeleteOnSuccess.Value)
                {
                    await Store.DeleteAsync(task);
                    deleted = true;
                }

                task.SucceededAt = now;
            }

            // repeat if we reached a final state on our task
            if ((task.SucceededAt.HasValue || task.FailedAt.HasValue) && task.NextOccurrence != null)
            {
                var shouldRepeat = success && task.ContinueOnSuccess ||
                                   !success && task.ContinueOnFailure ||
                                   exception != null && task.ContinueOnError;

                if (shouldRepeat)
                {
                    var nextOccurrence = task.NextOccurrence;

                    var clone = new BackgroundTask
                    {
                        Priority = task.Priority,
                        Handler = task.Handler,
                        DeleteOnSuccess = task.DeleteOnSuccess,
                        DeleteOnFailure = task.DeleteOnFailure,
                        DeleteOnError = task.DeleteOnError,
                        Expression = task.Expression,
                        Start = task.Start,
                        End = task.End,
                        ContinueOnSuccess = task.ContinueOnSuccess,
                        ContinueOnFailure = task.ContinueOnFailure,
                        ContinueOnError = task.ContinueOnError,
                        RunAt = nextOccurrence.GetValueOrDefault(),
                        MaximumAttempts = task.MaximumAttempts,
                        MaximumRuntime = task.MaximumRuntime,
                        Tags = task.Tags
                    };

                    await Store.SaveAsync(clone);
                }
            }

            if (deleted)
            {
                return;
            }

            // unlock for other workers
            task.LockedAt = null;
            task.LockedBy = null;
            await Store.SaveAsync(task);
        }

        private static bool TaskIsTerminal(BackgroundTask task)
        {
            return task.Attempts >= task.MaximumAttempts;
        }

        private async Task<(bool, Exception)> PerformAsync(BackgroundTask task)
        {
            // Acquire the handler:
            var handler = CreateHandler(task);

            if (handler == null)
            {
                task.LastError = ErrorStrings.InvalidHandler;
                return (false, null);
            }

            var hooks = GetOrCreateMethodHooks(handler);
            var context = ProvisionExecutionContext(_cancel.Token);

            _pending.TryAdd(handler, hooks);

            Exception exception;
            try
            {
                // Before:
                if (hooks.OnBefore != null)
                {
                    await hooks.OnBefore.BeforeAsync(context);
                }

                // Handler:
                if (context.Continue)
                {
                    await handler.PerformAsync(context);
                }

                // Success:
                if (context.Successful && hooks.OnSuccess != null)
                {
                    await hooks.OnSuccess.SuccessAsync(context);
                }

                // Failure:
                if (TaskIsTerminal(task) && hooks.OnFailure != null)
                {
                    await hooks.OnFailure.FailureAsync(context);
                }

                // After:
                if (hooks.OnAfter != null)
                {
                    await hooks.OnAfter?.AfterAsync(context);
                }

                exception = null;
            }
            catch (OperationCanceledException e)
            {
                task.LastError = "Cancelled";
                exception = e;
            }
            catch (Exception e)
            {
                task.LastError = e.Message;
                hooks?.OnError?.ErrorAsync(context, e);
                exception = e;
            }
            finally
            {
                _pending.TryRemove(handler, out _);
            }

            return (context.Successful, exception);
        }

        private Handler CreateHandler(BackgroundTask task)
        {
            try
            {
                var handlerInfo = Serializer.Deserialize<HandlerInfo>(task.Handler);

                var typeName = $"{handlerInfo.Namespace}.{handlerInfo.Entrypoint}";
                var type = _typeResolver.FindByFullName(typeName) ?? _typeResolver.FindFirstByName(typeName);
                if (type == null)
                    return null;

                var instance = _backgroundServices.AutoResolve(type);

                return instance == null ? null : TryWrapHook<Handler>(instance);
            }
            catch (Exception e)
            {
                _logger.Error(() => "Error creating handler for task", e);
                throw;
            }
        }

        // ReSharper disable once InconsistentNaming
        private static HandlerHooks GetOrCreateMethodHooks(Handler handler)
        {
            var handlerType = handler.GetType();
            if (!MethodCache.TryGetValue(handlerType, out var methods))
            {
                MethodCache.Add(handlerType,
                    methods = new HandlerHooks
                    {
                        Handler = handler,
                        OnBefore = TryWrapHook<Before>(handler),
                        OnAfter = TryWrapHook<After>(handler),
                        OnSuccess = TryWrapHook<Success>(handler),
                        OnFailure = TryWrapHook<Failure>(handler),
                        OnError = TryWrapHook<Error>(handler),
                        OnHalt = TryWrapHook<Halt>(handler)
                    });
            }

            return methods;
        }

        private static T TryWrapHook<T>(object instance) where T : class
        {
            var prototype = typeof(T).GetMethods();
            var example = instance.GetType().GetMethods();
            var match = prototype.Any(l => example.Any(r => AreMethodsDuckEquivalent(l, r)))
                ? instance.ActLike<T>()
                : null;
            return match;
        }

        private static bool AreMethodsDuckEquivalent(MethodInfo left, MethodInfo right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            if (!left.Name.Equals(right.Name))
            {
                return false;
            }

            if (left.Equals(right) || left.GetHashCode() == right.GetHashCode())
            {
                return true;
            }

            var lp = left.GetParameters();
            var rp = right.GetParameters();
            if (lp.Length != rp.Length)
            {
                return false;
            }

            if (lp.Where((t, i) => t.ParameterType != rp[i].ParameterType).Any())
            {
                return false;
            }

            return left.ReturnType == right.ReturnType;
        }

        private TaskScheduler AcquireScheduler(BackgroundTask task)
        {
            if (_schedulers.TryGetValue(task.Priority, out var scheduler))
            {
                return scheduler;
            }

            scheduler = _scheduler.ActivateNewQueue(task.Priority);
            var factory = new TaskFactory(_cancel.Token, TaskCreationOptions.LongRunning,
                TaskContinuationOptions.LongRunning, scheduler);
            _schedulers.TryAdd(task.Priority, scheduler);
            _factories.TryAdd(scheduler, factory);

            return scheduler;
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_cancel != null)
            {
                _cancel.Cancel();
                _cancel.Token.WaitHandle.WaitOne();
                _cancel.Dispose();
                _cancel = null;
            }

            _factories.Clear();
            _schedulers.Clear();

            _scheduler.Dispose();
            _scheduler = null;

            _background?.Dispose();
            _background = null;

            _maintenance?.Dispose();
            _maintenance = null;
        }

        public DateTimeOffset GetCurrentTime()
        {
            return _timestamps.GetCurrentTime();
        }

        public DateTimeOffset UtcNow => _timestamps.UtcNow;
    }
}
