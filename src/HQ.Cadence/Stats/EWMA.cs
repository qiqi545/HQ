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
using HQ.Cadence.Internal;

namespace HQ.Cadence.Stats
{
    /// <summary>
    ///     An exponentially-weighted moving average
    /// </summary>
    /// <see href="http://www.teamquest.com/pdfs/whitepaper/ldavg1.pdf" />
    /// <see href="http://www.teamquest.com/pdfs/whitepaper/ldavg2.pdf" />
    public class EWMA
    {
        private static readonly double M1Alpha = 1 - Math.Exp(-5 / 60.0);
        private static readonly double M5Alpha = 1 - Math.Exp(-5 / 60.0 / 5);
        private static readonly double M15Alpha = 1 - Math.Exp(-5 / 60.0 / 15);
        private readonly double _alpha;
        private readonly double _interval;

        private readonly AtomicLong _uncounted = new AtomicLong(0);
        private volatile bool _initialized;
        private VolatileDouble _rate;

        /// <summary>
        ///     Create a new EWMA with a specific smoothing constant.
        /// </summary>
        /// <param name="alpha">The smoothing constant</param>
        /// <param name="interval">The expected tick interval</param>
        /// <param name="intervalUnit">The time unit of the tick interval</param>
        public EWMA(double alpha, long interval, TimeUnit intervalUnit)
        {
            _interval = intervalUnit.ToNanos(interval);
            _alpha = alpha;
        }

        /// <summary>
        ///     Creates a new EWMA which is equivalent to the UNIX one minute load average and which expects to be ticked every 5
        ///     seconds.
        /// </summary>
        public static EWMA OneMinuteEWMA()
        {
            return new EWMA(M1Alpha, 5, TimeUnit.Seconds);
        }

        /// <summary>
        ///     Creates a new EWMA which is equivalent to the UNIX five minute load average and which expects to be ticked every 5
        ///     seconds.
        /// </summary>
        /// <returns></returns>
        public static EWMA FiveMinuteEWMA()
        {
            return new EWMA(M5Alpha, 5, TimeUnit.Seconds);
        }

        /// <summary>
        ///     Creates a new EWMA which is equivalent to the UNIX fifteen minute load average and which expects to be ticked every
        ///     5 seconds.
        /// </summary>
        /// <returns></returns>
        public static EWMA FifteenMinuteEWMA()
        {
            return new EWMA(M15Alpha, 5, TimeUnit.Seconds);
        }

        /// <summary>
        ///     Update the moving average with a new value.
        /// </summary>
        /// <param name="n"></param>
        public void Update(long n)
        {
            _uncounted.AddAndGet(n);
        }

        /// <summary>
        ///     Mark the passage of time and decay the current rate accordingly.
        /// </summary>
        public void Tick()
        {
            var count = _uncounted.GetAndSet(0);
            var instantRate = count / _interval;
            if (_initialized)
            {
                _rate += _alpha * (instantRate - _rate);
            }
            else
            {
                _rate.Set(instantRate);
                _initialized = true;
            }
        }

        /// <summary>
        ///     Returns the rate in the given units of time.
        /// </summary>
        public double Rate(TimeUnit rateUnit)
        {
            return _rate * rateUnit.ToNanos(1);
        }
    }
}
