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
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Extensions.Scheduling.Internal
{
    /// <summary>
    ///     Extensions for convenient wrappers around delegates to produce a continuous stream of objects.
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        ///     Executes the delegate continuously until cancelled by the subscriber.
        ///     <remarks>
        ///         It's important to add an additional buffer or window to this to avoid busy waiting, or use the built-in
        ///         interval.
        ///     </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegate"></param>
        /// <param name="interval"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IObservable<T> AsContinuousObservable<T>(this Func<T> @delegate, TimeSpan? interval = null,
            TaskScheduler scheduler = null)
        {
            scheduler = scheduler ?? TaskScheduler.Default;

            return new Func<CancellationToken, T>(token => @delegate()).AsContinuousObservable(interval, scheduler);
        }

        /// <summary>
        ///     Executes the delegate continuously until cancelled by the subscriber.
        ///     <remarks>
        ///         It's important to add an additional buffer or window to this to avoid busy waiting, or use the built-in
        ///         interval.
        ///     </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegate"></param>
        /// <param name="interval"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IObservable<T> AsContinuousObservable<T>(this Func<IEnumerable<T>> @delegate,
            TimeSpan? interval = null, TaskScheduler scheduler = null)
        {
            scheduler = scheduler ?? TaskScheduler.Default;

            if (interval.HasValue)
            {
                return Observable.Create<T>((observer, cancellationToken) => scheduler.Run(async () =>
                {
                    await Task.Delay(interval.Value, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var items = @delegate();
                        foreach (var item in items)
                        {
                            observer.OnNext(item);
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }, cancellationToken)).Repeat();
            }

            return Observable.Create<T>((observer, cancelToken) => scheduler.Run(() =>
            {
                if (!cancelToken.IsCancellationRequested)
                {
                    var items = @delegate();
                    foreach (var item in items)
                    {
                        observer.OnNext(item);
                    }
                }

                cancelToken.ThrowIfCancellationRequested();
            }, cancelToken)).Repeat();
        }

        /// <summary>
        ///     Executes the delegate continuously until cancelled by the subscriber.
        ///     <remarks>
        ///         It's important to add an additional buffer or window to this to avoid busy waiting, or use the built-in
        ///         interval.
        ///     </remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delegate"></param>
        /// <param name="interval"></param>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static IObservable<T> AsContinuousObservable<T>(this Func<CancellationToken, T> @delegate,
            TimeSpan? interval = null, TaskScheduler scheduler = null)
        {
            scheduler = scheduler ?? TaskScheduler.Default;

            if (interval.HasValue)
            {
                return Observable.Create<T>((observer, cancellationToken) => scheduler.Run(async () =>
                {
                    await Task.Delay(interval.Value, cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        observer.OnNext(@delegate(cancellationToken));
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }, cancellationToken)).Repeat();
            }

            return Observable.Create<T>((observer, cancellationToken) => scheduler.Run(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    var items = @delegate(cancellationToken);
                    observer.OnNext(items);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }, cancellationToken)).Repeat();
        }
    }
}
