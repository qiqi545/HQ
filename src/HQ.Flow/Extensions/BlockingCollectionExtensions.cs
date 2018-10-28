// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Flow.Extensions
{
	internal static class BlockingCollectionExtensions
	{
		public static IObservable<IList<T>> AsBatchingObservable<T>(this BlockingCollection<T> sequence, int n, CancellationToken cancellationToken = default)
		{
			return sequence.AsConsumingObservable(cancellationToken).Buffer(n);
		}

		public static IObservable<IList<T>> AsBatchingObservable<T>(this BlockingCollection<T> sequence, TimeSpan w, CancellationToken cancellationToken = default)
		{
			return sequence.AsConsumingObservable(cancellationToken).Buffer(w);
		}

		public static IObservable<IList<T>> AsBatchingObservable<T>(this BlockingCollection<T> sequence, int n, TimeSpan w, CancellationToken cancellationToken = default)
		{
			return sequence.AsConsumingObservable(cancellationToken).Buffer(w, n);
		}

		public static IObservable<T> AsConsumingObservable<T>(this BlockingCollection<T> sequence, CancellationToken cancellationToken = default)
		{
			var subject = new Subject<T>();
			var token = new CancellationToken();
			var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
			var consumingTask = new Task(() =>
			{
				while (!sequence.IsCompleted)
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

				subject.OnCompleted();
			}, TaskCreationOptions.LongRunning);

			return new TaskAwareObservable<T>(subject, consumingTask, tokenSource);
		}

		// Original source from: http://codereview.stackexchange.com/questions/11377/implementation-of-a-throttled-concurrentqueue-rx-observer
		public static IObservable<T> AsRateLimitedObservable<T>(this BlockingCollection<T> sequence, int occurrences, TimeSpan timeUnit, CancellationToken cancellationToken = default)
		{
			var subject = new Subject<T>();
			var token = new CancellationToken();
			var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token);
			var consumingTask = new Task(() =>
			{
				using (var throttle = new Throttle(occurrences, timeUnit))
				{
					while (!sequence.IsCompleted)
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

					subject.OnCompleted();
				}
			}, TaskCreationOptions.LongRunning);

			return new TaskAwareObservable<T>(subject, consumingTask, tokenSource);
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
				if (_task.Status == TaskStatus.Created) _task.Start();
				return disposable;
			}
		}

		// Original source from: http://www.jackleitch.net/2010/10/better-rate-limiting-with-dot-net/
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
					timeUntilNextCheck = unchecked(exitTime - Environment.TickCount);
				else
					timeUntilNextCheck = TimeUnitMilliseconds;
				_exitTimer.Change(timeUntilNextCheck, -1);
			}

			public bool WaitToProceed(int millisecondsTimeout = Timeout.Infinite)
			{
				var entered = _semaphore.Wait(millisecondsTimeout);
				if (entered)
				{
					var exitTime = unchecked(Environment.TickCount + TimeUnitMilliseconds);
					_exitTimes.Enqueue(exitTime);
				}
				return entered;
			}
			
			protected virtual void Dispose(bool isDisposing)
			{
				if (!isDisposing || _isDisposed) return;
				_semaphore.Dispose();
				_exitTimer.Dispose();
				_isDisposed = true;
			}
		}
	}
}