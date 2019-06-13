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
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Internal;
using ImpromptuInterface;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Extensions.Scheduling.Models
{
    public class BackgroundTaskHost : IDisposable, IServerTimestampService
    {
        private const string NoHandlerState = "{}";

        private static readonly IDictionary<HandlerInfo, Handler> HandlerCache =
            new ConcurrentDictionary<HandlerInfo, Handler>();

        private static readonly IDictionary<Type, HandlerHooks> MethodCache =
            new ConcurrentDictionary<Type, HandlerHooks>();

        private readonly ConcurrentDictionary<TaskScheduler, TaskFactory> _factories;
        private readonly ConcurrentDictionary<Handler, HandlerHooks> _pending;
        private readonly ConcurrentDictionary<int, TaskScheduler> _schedulers;

        private readonly IServerTimestampService _timestamps;
        private readonly ITypeResolver _typeResolver;

        private CancellationTokenSource _cancel;
        private QueuedTaskScheduler _scheduler;
        private PushQueue<IEnumerable<BackgroundTask>> _background;
        private PushQueue<IEnumerable<BackgroundTask>> _maintenance;

        private readonly ISafeLogger<BackgroundTaskHost> _logger;
        private readonly IOptionsMonitor<BackgroundTaskOptions> _settings;

        public BackgroundTaskHost(IServerTimestampService timestamps, IBackgroundTaskStore store, IBackgroundTaskSerializer serializer, ITypeResolver typeResolver, IOptionsMonitor<BackgroundTaskOptions> settings, ISafeLogger<BackgroundTaskHost> logger)
        {
            _timestamps = timestamps;
            Store = store;
            Serializer = serializer;
            _typeResolver = typeResolver;
            _settings = settings;
            _logger = logger;
            settings.OnChange(OnSettingsChanged);

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
                var settings = _settings.CurrentValue;

                var @readonly = new BackgroundTaskOptions
                {
                    DelayTasks = settings.DelayTasks,
                    Concurrency = settings.Concurrency,
                    SleepInterval = settings.SleepInterval,
                    IntervalFunction = settings.IntervalFunction,
                    ReadAhead = settings.ReadAhead,
                    MaximumAttempts = settings.MaximumAttempts,
                    MaximumRuntime = settings.MaximumRuntime,
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
            _logger.Info(() => "Background task settings changed, recycling the host.");
            Stop();
            Start();
        }

        private IEnumerable<BackgroundTask> EnqueueTasks()
        {
            return Store.GetAndLockNextAvailable(_settings.CurrentValue.ReadAhead);
        }

        private IEnumerable<BackgroundTask> HangingTasks()
        {
            return Store.GetHangingTasks();
        }

        public void Start(bool immediate = false)
        {
            if (_scheduler == null)
            {
                _scheduler = new QueuedTaskScheduler(ResolveConcurrency());
            }

            _background.Produce(EnqueueTasks, _settings.CurrentValue.SleepInterval);
            _background.Start(immediate);

            _maintenance.Produce(HangingTasks, TimeSpan.FromMinutes(1));
            _maintenance.Start(immediate);
        }

        private int ResolveConcurrency()
        {
            return _settings.CurrentValue.Concurrency == 0
                ? Environment.ProcessorCount
                : _settings.CurrentValue.Concurrency;
        }

        public void Stop(CancellationToken cancellationToken = default, bool immediate = false)
        {
            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = ResolveConcurrency()
            };

            Parallel.ForEach(_pending.Where(entry => entry.Value.OnHalt != null), options,
                e => { e.Value.OnHalt.Halt(immediate); });

            _pending.Clear();

            _scheduler?.Dispose();
            _scheduler = null;

            _background.Stop(immediate);
            _maintenance.Stop(immediate);
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

                if (JobWillFail(task))
                {
                    if (task.DeleteOnFailure.HasValue && task.DeleteOnFailure.Value)
                    {
                        Store.Delete(task);
                    }
                    else
                    {
                        task.FailedAt = now;
                    }
                }
                else
                {
                    task.RunAt += _settings.CurrentValue.IntervalFunction(task.Attempts);
                }

                Store.Save(task);
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

                var started = taskFactory.StartNew(() => { AttemptTask(task); }, cancel.Token);

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

        internal bool AttemptTask(BackgroundTask task, bool persist = true)
        {
            if (_cancel.IsCancellationRequested)
            {
                return false;
            }

            var success = AttemptCycle(task, out var exception);

            if (persist)
            {
                UpdateTask(task, success, exception);
            }

            _cancel.Token.ThrowIfCancellationRequested();

            return success;
        }

        private bool AttemptCycle(BackgroundTask task, out Exception exception)
        {
            task.Attempts++;

            var success = Perform(task, out exception);
            if (!success)
            {
                task.RunAt = _timestamps.GetCurrentTime() + _settings.CurrentValue.IntervalFunction(task.Attempts);
            }

            return success;
        }

        private void UpdateTask(BackgroundTask task, bool success, Exception exception)
        {
            var deleted = false;

            var now = _timestamps.GetCurrentTime();

            if (!success)
            {
                if (JobWillFail(task))
                {
                    if (task.DeleteOnFailure.HasValue && task.DeleteOnFailure.Value)
                    {
                        Store.Delete(task);
                        deleted = true;
                    }

                    task.FailedAt = now;
                }
            }
            else
            {
                if (task.DeleteOnSuccess.HasValue && task.DeleteOnSuccess.Value)
                {
                    Store.Delete(task);
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

                    Store.Save(clone);
                }
            }

            if (deleted)
            {
                return;
            }

            // unlock for other workers
            task.LockedAt = null;
            task.LockedBy = null;
            Store.Save(task);
        }

        private static bool JobWillFail(BackgroundTask task)
        {
            return task.Attempts >= task.MaximumAttempts;
        }

        private bool Perform(BackgroundTask task, out Exception exception)
        {
            var success = false;

            // Acquire the handler:
            var handler = CreateOrGetHandler(task);

            if (handler == null)
            {
                task.LastError = ErrorStrings.InvalidHandler;
                exception = null;
                return false;
            }

            // Acquire and cache method manifest:
            var methods = CacheOrCreateMethods(handler);

            _pending.TryAdd(handler, methods);
            try
            {
                // Before:
                var before = methods.OnBefore?.Before();

                // Handler:
                if (!before.HasValue || before.Value)
                {
                    success = handler.Perform();
                }

                // Success:
                if (success)
                {
                    methods.OnSuccess?.Success();
                }

                // Failure:
                if (JobWillFail(task))
                {
                    methods.OnFailure?.Failure();
                }

                // After:
                methods.OnAfter?.After();

                exception = null;
            }
            catch (OperationCanceledException oce)
            {
                task.LastError = "Cancelled";
                exception = oce;
            }
            catch (Exception ex)
            {
                task.LastError = ex.Message;
                methods?.OnError?.Error(ex);
                exception = ex;
            }
            finally
            {
                _pending.TryRemove(handler, out _);
            }

            return success;
        }

        private Handler CreateOrGetHandler(BackgroundTask task)
        {
            try
            {
                var handlerInfo = Serializer.Deserialize<HandlerInfo>(task.Handler);

                if (!HandlerCache.TryGetValue(handlerInfo, out var handler))
                {
                    var typeName = $"{handlerInfo.Namespace}.{handlerInfo.Entrypoint}";
                    var type = _typeResolver.FindByFullName(typeName) ?? _typeResolver.FindFirstByName(typeName);
                    if (type != null)
                    {
                        object instance;
                        if (!string.IsNullOrWhiteSpace(handlerInfo.Instance) &&
                            !handlerInfo.Instance.Equals(NoHandlerState))
                        {
                            try
                            {
                                instance = Serializer.Deserialize(handlerInfo.Instance, type);
                            }
                            catch (Exception)
                            {
                                instance = null;
                            }
                        }
                        else
                        {
                            instance = Instancing.CreateInstance(type);
                        }

                        if (instance == null)
                        {
                            return null;
                        }

                        handler = TryWrapHook<Handler>(instance);
                        if (handler != null)
                        {
                            HandlerCache.Add(handlerInfo, handler);
                        }
                    }
                }

                return handler;
            }
            catch (Exception e)
            {
                _logger.Error(() => "Error creating handler for task", e);
                throw;
            }
        }

        private static HandlerHooks CacheOrCreateMethods(Handler handler)
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
