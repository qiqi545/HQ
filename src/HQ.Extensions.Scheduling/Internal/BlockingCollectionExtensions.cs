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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Extensions.Scheduling.Internal
{
    internal static class BlockingCollectionExtensions
    {
        public static IObservable<IList<T>> AsBatchingObservable<T>(this BlockingCollection<T> sequence, int n,
            CancellationToken cancellationToken)
        {
            return sequence.AsConsumingObservable(cancellationToken).Buffer(n);
        }

        public static IObservable<IList<T>> AsBatchingObservable<T>(this BlockingCollection<T> sequence, TimeSpan w,
            CancellationToken cancellationToken)
        {
            return sequence.AsConsumingObservable(cancellationToken).Buffer(w);
        }

        public static IObservable<IList<T>> AsBatchingObservable<T>(this BlockingCollection<T> sequence, int n,
            TimeSpan w, CancellationToken cancellationToken)
        {
            return sequence.AsConsumingObservable(cancellationToken).Buffer(w, n);
        }

        public static IObservable<T> AsConsumingObservable<T>(this BlockingCollection<T> sequence,
            CancellationToken cancellationToken)
        {
            var subject = new Subject<T>();
            var token = new CancellationToken();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
            var consumingTask = new Task(() =>
            {
                while (!sequence.IsCompleted)
                {
                    try
                    {
                        var item = sequence.Take(cancellationToken);
                        try
                        {
                            subject.OnNext(item);
                        }
                        catch (Exception ex)
                        {
                            subject.OnError(ex);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                subject.OnCompleted();
            }, TaskCreationOptions.LongRunning);

            return new TaskAwareObservable<T>(subject, consumingTask, tokenSource);
        }

        // Original source from: http://codereview.stackexchange.com/questions/11377/implementation-of-a-throttled-concurrentqueue-rx-observer
        public static IObservable<T> AsRateLimitedObservable<T>(this BlockingCollection<T> sequence, int occurrences,
            TimeSpan timeUnit, CancellationToken cancellationToken)
        {
            var subject = new Subject<T>();
            var token = new CancellationToken();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
            var consumingTask = new Task(() =>
            {
                using (var throttle = new Throttle(occurrences, timeUnit))
                {
                    while (!sequence.IsCompleted)
                    {
                        try
                        {
                            var item = sequence.Take(cancellationToken);
                            throttle.WaitToProceed();
                            try
                            {
                                subject.OnNext(item);
                            }
                            catch (Exception ex)
                            {
                                subject.OnError(ex);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }

                    subject.OnCompleted();
                }
            }, TaskCreationOptions.LongRunning);

            return new TaskAwareObservable<T>(subject, consumingTask, tokenSource);
        }

        // Original source from: http://code.msdn.microsoft.com/ParExtSamples
        // See: http://blogs.msdn.com/b/pfxteam/archive/2010/04/06/9990420.aspx
        // Existing issue: http://connect.microsoft.com/VisualStudio/feedback/details/674705/blockingcollection-getconsumingenumerable-and-parallel-foreach-hang
        // Forum post: http://social.msdn.microsoft.com/Forums/en-US/parallelextensions/thread/bcb4c46b-8ac4-423d-a7e6-7dacaa574e5c
        public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection)
        {
            return new BlockingCollectionPartitioner<T>(collection);
        }

        // Original source from: http://codereview.stackexchange.com/questions/11377/implementation-of-a-throttled-concurrentqueue-rx-observer
        private class TaskAwareObservable<T> : IObservable<T>, IDisposable
        {
            private readonly Subject<T> _subject;
            private readonly Task _task;
            private readonly CancellationTokenSource _taskCancellationTokenSource;

            public TaskAwareObservable(Subject<T> subject, Task task, CancellationTokenSource tokenSource)
            {
                _task = task;
                _subject = subject;
                _taskCancellationTokenSource = tokenSource;
            }

            public void Dispose()
            {
                _taskCancellationTokenSource.Cancel();
                _task.Wait();

                _taskCancellationTokenSource.Dispose();

                _subject.Dispose();
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                var disposable = _subject.Subscribe(observer);
                if (_task.Status == TaskStatus.Created)
                {
                    _task.Start();
                }

                return disposable;
            }
        }

        // Original source from: http://www.pennedobjects.com/2010/10/better-rate-limiting-with-dot-net/
        internal class Throttle : IDisposable
        {
            private readonly Timer _exitTimer;
            private readonly ConcurrentQueue<int> _exitTimes;
            private readonly SemaphoreSlim _semaphore;
            private bool _isDisposed;

            public Throttle(int occurrences, TimeSpan timeUnit)
            {
                Occurrences = occurrences;
                TimeUnitMilliseconds = (int) timeUnit.TotalMilliseconds;

                _semaphore = new SemaphoreSlim(Occurrences, Occurrences);
                _exitTimes = new ConcurrentQueue<int>();
                _exitTimer = new Timer(ExitTimerCallback, null, TimeUnitMilliseconds, -1);
            }

            public int Occurrences { get; }
            public int TimeUnitMilliseconds { get; }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void ExitTimerCallback(object state)
            {
                int exitTime;
                while (_exitTimes.TryPeek(out exitTime) && unchecked(exitTime - Environment.TickCount) <= 0)
                {
                    _semaphore.Release();
                    _exitTimes.TryDequeue(out exitTime);
                }

                int timeUntilNextCheck;
                if (_exitTimes.TryPeek(out exitTime))
                {
                    timeUntilNextCheck = unchecked(exitTime - Environment.TickCount);
                }
                else
                {
                    timeUntilNextCheck = TimeUnitMilliseconds;
                }

                _exitTimer.Change(timeUntilNextCheck, -1);
            }

            public bool WaitToProceed(int millisecondsTimeout)
            {
                var entered = _semaphore.Wait(millisecondsTimeout);
                if (entered)
                {
                    var exitTime = unchecked(Environment.TickCount + TimeUnitMilliseconds);
                    _exitTimes.Enqueue(exitTime);
                }

                return entered;
            }

            public bool WaitToProceed(TimeSpan timeout)
            {
                return WaitToProceed((int) timeout.TotalMilliseconds);
            }

            public void WaitToProceed()
            {
                WaitToProceed(Timeout.Infinite);
            }

            protected virtual void Dispose(bool isDisposing)
            {
                if (!isDisposing || _isDisposed)
                {
                    return;
                }

                _semaphore.Dispose();
                _exitTimer.Dispose();
                _isDisposed = true;
            }
        }

        private class BlockingCollectionPartitioner<T> : Partitioner<T>
        {
            private readonly BlockingCollection<T> _collection;

            internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
            {
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            }

            public override bool SupportsDynamicPartitions => true;

            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                var dynamicPartitioner = GetDynamicPartitions();

                return Enumerable.Range(0, partitionCount).Select(_ => dynamicPartitioner.GetEnumerator()).ToArray();
            }

            public override IEnumerable<T> GetDynamicPartitions()
            {
                return _collection.GetConsumingEnumerable();
            }
        }
    }
}
