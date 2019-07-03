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
using System.Globalization;

namespace HQ.Common.Numerics
{
    /// <summary>A signed 16.16 fixed-point number</summary>
    public struct FixedSingle : IComparable<FixedSingle>, IEquatable<FixedSingle>, IFormattable
    {
        private const int N = 16;

        private readonly int _value;

        public int WholePart => _value >> N;
        public int Fraction => _value & ((1 << N) - 1);
        
        private FixedSingle(int value)
        {
            _value = value;
        }

        
        #region Operators

        public static FixedSingle operator +(FixedSingle x)
        {
            return x;
        }

        public static FixedSingle operator -(FixedSingle x)
        {
            return new FixedSingle(-x._value);
        }

        public static FixedSingle operator +(FixedSingle x, FixedSingle y)
        {
            return new FixedSingle(x._value + y._value);
        }

        public static FixedSingle operator -(FixedSingle x, FixedSingle y)
        {
            return new FixedSingle(x._value - y._value);
        }

        public static FixedSingle operator *(FixedSingle x, FixedSingle y)
        {
            return new FixedSingle((int)((x._value * (long)y._value) >> N));
        }

        public static FixedSingle operator *(FixedSingle x, int y)
        {
            return new FixedSingle(x._value * y);
        }

        public static FixedSingle operator *(int x, FixedSingle y)
        {
            return new FixedSingle(x * y._value);
        }

        public static FixedSingle operator /(FixedSingle x, FixedSingle y)
        {
            return new FixedSingle((int)(((long)x._value << N) / y._value));
        }

        public static FixedSingle operator >>(FixedSingle x, int shift)
        {
            return new FixedSingle(x._value >> shift);
        }

        public static FixedSingle operator <<(FixedSingle x, int shift)
        {
            return new FixedSingle(x._value << shift);
        }

        #endregion

        #region Equality

        public static bool operator ==(FixedSingle x, FixedSingle y)
        {
            return x._value == y._value;
        }

        public static bool operator !=(FixedSingle x, FixedSingle y)
        {
            return x._value != y._value;
        }

        public static bool operator >(FixedSingle x, FixedSingle y)
        {
            return x._value > y._value;
        }

        public static bool operator <(FixedSingle x, FixedSingle y)
        {
            return x._value < y._value;
        }

        public static bool operator >=(FixedSingle x, FixedSingle y)
        {
            return x._value >= y._value;
        }

        public static bool operator <=(FixedSingle x, FixedSingle y)
        {
            return x._value <= y._value;
        }

        #endregion

        #region API

        public int Size => sizeof(int);

        public static readonly FixedSingle One = new FixedSingle(N * N * N * N);

        public static readonly FixedSingle Zero = default;

        public static readonly FixedSingle MaxValue = new FixedSingle(int.MaxValue);

        public static readonly FixedSingle MinValue = new FixedSingle(int.MinValue);
        
        public int CompareTo(FixedSingle other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(FixedSingle other)
        {
            return _value.Equals(other._value);
        }

        public override bool Equals(object obj)
        {
            return obj is FixedSingle @fixed && @fixed._value == _value;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static explicit operator float(FixedSingle value)
        {
            return (float) value._value / One._value;
        }

        public static explicit operator double(FixedSingle value)
        {
            return (double) value._value / One._value;
        }

        public static explicit operator decimal(FixedSingle value)
        {
            return (decimal) value._value / One._value;
        }

        public static explicit operator FixedSingle(float value)
        {
            return new FixedSingle((int)(value * One._value));
        }

        public static explicit operator FixedSingle(double value)
        {
            return new FixedSingle((int)(value * One._value));
        }

        public static explicit operator FixedSingle(decimal value)
        {
            return new FixedSingle((int)(value * One._value));
        }

        #endregion

        #region Formatting

        public override string ToString()
        {
            return ((decimal)this).ToString(CultureInfo.InvariantCulture);
        }
        
        public string ToString(string format)
        {
            return ((decimal)this).ToString(format);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ((decimal)this).ToString(format, formatProvider);
        }

        public static bool TryParse(string s, out FixedSingle result)
        {
            if (!decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
            {
                result = default;
                return false;
            }
            result = (FixedSingle) d;
            return true;
        }

        public static FixedSingle Parse(string s)
        {
            return (FixedSingle) decimal.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
