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
using System.Numerics;

namespace HQ.Common.Numerics
{
    public struct FixedNumber : IComparable<FixedNumber>, IEquatable<FixedNumber>
    {
        private BigInteger _value;
        private int _scale;

        public int WholePart
        {
            get
            {
                Split(out var wholePart, out _);
                return wholePart;
            }
        }

        public int Fraction
        {
            get
            {
                Split(out _, out var fraction);
                return fraction;
            }
        }

        public int Precision
        {
            get => _value == 0 ? 1 : 1 + (int)Math.Log10(Math.Abs((long)_value));
            set => Scale = value - _scale;
        }

        public int Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                {
                    return;
                }

                BigInteger newValue;
                if (_scale < value)
                {
                    var exponent = (byte) (value - _scale);
                    newValue = _value * BigInteger.Pow(10, exponent);
                }
                else
                {
                    var exponent = (byte) (_scale - value);
                    var divisor = BigInteger.Pow(10, exponent);
                    newValue = BigInteger.DivRem(_value, divisor, out _);
                }

                _value = newValue;
                _scale = value;
            }
        }

        public int Sign => _value.Sign;

        public FixedNumber(int value, int scale = 0)
        {
            _value = value;
            _scale = scale;
        }

        public FixedNumber(long value, int scale = 0)
        {
            _value = value;
            _scale = scale;
        }

        private FixedNumber(BigInteger value, int scale)
        {
            _value = value;
            _scale = scale;
        }

        #region Operators

        public static FixedNumber operator +(FixedNumber x)
        {
            return x;
        }

        public static FixedNumber operator -(FixedNumber x)
        {
            var value = -x._value;

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator -(FixedNumber x, FixedNumber y)
        {
            var value = (int)(x._value - y._value);

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator -(FixedNumber x, int y)
        {
            var value = x._value - new BigInteger(y * Math.Pow(10, x._scale));

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator +(FixedNumber x, FixedNumber y)
        {
            var value = x._value + y._value;

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator +(FixedNumber x, int y)
        {
            var value = x._value + new BigInteger(y * Math.Pow(10, x._scale));

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator *(FixedNumber x, FixedNumber y)
        {
            var value = (int)(x._value * y._value);

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator *(FixedNumber x, int y)
        {
            var value = (int) (x._value * y);

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator *(int x, FixedNumber y)
        {
            var value = (int) (x * y._value);

            return new FixedNumber(value, y._scale);
        }

        public static FixedNumber operator /(FixedNumber x, FixedNumber y)
        {
            var value = (int)(x._value / y._value);

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator >>(FixedNumber x, int shift)
        {
            var value = (int) x._value >> shift;

            return new FixedNumber(value, x._scale);
        }

        public static FixedNumber operator <<(FixedNumber x, int shift)
        {
            var value = (int)x._value << shift;

            return new FixedNumber(value, x._scale);
        }

        #endregion

        #region Equality

        public static bool operator ==(FixedNumber x, FixedNumber y)
        {
            return x._value == y._value;
        }

        public static bool operator !=(FixedNumber x, FixedNumber y)
        {
            return x._value != y._value;
        }

        public static bool operator >(FixedNumber x, FixedNumber y)
        {
            return x._value > y._value;
        }

        public static bool operator <(FixedNumber x, FixedNumber y)
        {
            return x._value < y._value;
        }

        public static bool operator >=(FixedNumber x, FixedNumber y)
        {
            return x._value >= y._value;
        }

        public static bool operator <=(FixedNumber x, FixedNumber y)
        {
            return x._value <= y._value;
        }

        #endregion

        #region API

        public static readonly FixedNumber One = new FixedNumber(1);

        public static readonly FixedNumber Zero = default;

        public int CompareTo(FixedNumber other)
        {
            var compare = _value.CompareTo(other._value);
            return compare != 0 ? compare : _scale.CompareTo(other._scale);
        }

        public bool Equals(FixedNumber other)
        {
            return _value.Equals(other._value) && _scale.Equals(other._scale);
        }

        public override bool Equals(object obj)
        {
            return obj is FixedNumber @fixed && @fixed._value == _value && @fixed._scale == _scale;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_value.GetHashCode() * 397) ^ _scale;
            }
        }

        #endregion

        #region Formatting

        public override string ToString()
        {
            Split(out var wholePart, out var fraction);

            return fraction == 0 ? wholePart.ToString() : $"{wholePart}.{fraction.ToString($"d{Scale}")}".TrimEnd('0');
        }
        
        public static bool TryParse(string s, out FixedNumber result)
        {
            if (decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var @decimal))
            {
                var wholePart = (int) @decimal;
                var trimmed = (@decimal % 1.0m).ToString(CultureInfo.InvariantCulture).TrimEnd('0');
                var fractionalPart = trimmed.Substring(trimmed.IndexOf('.') + 1);
                result = new FixedNumber(int.Parse($"{wholePart}{fractionalPart}"), fractionalPart.Length);
                return true;
            }

            result = default;
            return false;
        }

        public static FixedNumber Parse(string s)
        {
            TryParse(s, out var result);
            return result;
        }

        #endregion

        private void Split(out int wholePart, out int fraction)
        {
            var divisor = BigInteger.Pow(10, Scale);
            wholePart = (int)BigInteger.DivRem(_value, divisor, out var remainder);
            fraction = (int)remainder;
        }
    }
}
