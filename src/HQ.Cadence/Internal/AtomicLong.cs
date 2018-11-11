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

using System.Threading;

namespace HQ.Cadence.Internal
{
    /// <summary>
    ///     Provides support for atomic operations around a <see cref="long" /> value
    /// </summary>
    internal class AtomicLong
    {
        private long _value;

        public AtomicLong()
        {
            Set(0);
        }

        public AtomicLong(long value)
        {
            Set(value);
        }

        /// <summary>
        ///     Get the current value
        /// </summary>
        public long Get()
        {
            return Interlocked.Read(ref _value);
        }

        /// <summary>
        ///     Set to the given value
        /// </summary>
        public void Set(long value)
        {
            Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        ///     Atomically add the given value to the current value
        /// </summary>
        public long AddAndGet(long amount)
        {
            Interlocked.Add(ref _value, amount);
            return Get();
        }

        /// <summary>
        ///     Atomically increments by one and returns the current value
        /// </summary>
        /// <returns></returns>
        public long IncrementAndGet()
        {
            Interlocked.Increment(ref _value);
            return Get();
        }

        /// <summary>
        ///     Atomically set the value to the given updated value if the current value == expected value
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="updated">The new value</param>
        /// <returns></returns>
        public bool CompareAndSet(long expected, long updated)
        {
            if (Get() == expected)
            {
                Set(updated);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Set to the given value and return the previous value
        /// </summary>
        public long GetAndSet(long value)
        {
            var previous = Get();
            Set(value);
            return previous;
        }

        /// <summary>
        ///     Adds the given value and return the previous value
        /// </summary>
        public long GetAndAdd(long value)
        {
            var previous = Get();
            Interlocked.Add(ref _value, value);
            return previous;
        }

        public static implicit operator AtomicLong(long value)
        {
            return new AtomicLong(value);
        }

        public static implicit operator long(AtomicLong value)
        {
            return value.Get();
        }
    }
}
