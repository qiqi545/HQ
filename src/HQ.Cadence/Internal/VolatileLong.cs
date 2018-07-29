// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading;

namespace HQ.Cadence.Internal
{
    /// <summary>
    /// Provides support for volatile operations around a <see cref="long" /> value
    /// </summary>
    internal struct VolatileLong
    {
        private long _value;

        public static VolatileLong operator +(VolatileLong left, VolatileLong right)
        {
            return Add(left, right);
        }

        private static VolatileLong Add(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() + right.Get());
            return left.Get();
        }

        public static VolatileLong operator -(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() - right.Get());
            return left.Get();
        }

        public static VolatileLong operator *(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() * right.Get());
            return left.Get();
        }

        public static VolatileLong operator /(VolatileLong left, VolatileLong right)
        {
            left.Set(left.Get() / right.Get());
            return left.Get();
        }

        private VolatileLong(VolatileLong value): this()
        {
            Set(value);
        }

        public void Set(long value)
        {
            Thread.VolatileWrite(ref _value, value);
        }

        public long Get()
        {
            return Thread.VolatileRead(ref _value);
        }

        public static implicit operator VolatileLong(long value)
        {
            var result = new VolatileLong();
            result.Set(value);
            return result;
        }

        public static implicit operator long(VolatileLong value)
        {
            return value.Get();
        }

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}