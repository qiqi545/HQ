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

namespace HQ.Extensions.Types
{
    partial struct Money
    {
        public static bool operator <=(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units <= right._units;
        }

        public static bool operator >=(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units >= right._units;
        }

        public static bool operator >(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units > right._units;
        }

        public static bool operator <(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            return left._units < right._units;
        }

        public static bool operator ==(Money left, Money right)
        {
            return left._units == right._units &&
                   left._places == right._places &&
                   left.CurrencyInfo == right.CurrencyInfo;
        }

        public static bool operator !=(Money left, Money right)
        {
            return !(left == right);
        }

        public static Money operator +(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            left._units += right._units;

            return left;
        }

        public static Money operator -(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            left._units -= right._units;

            return left;
        }

        public static Money operator *(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            var product = Convert.ToDouble(left._units) * Convert.ToDouble(right._units);

            var factor = Math.Pow(10, left._places * 2);

            product /= factor;

            var result = new Money(left.CurrencyInfo, product);

            return result;
        }

        public static Money operator /(Money left, Money right)
        {
            EnsureSameCurrency(left, right);

            HarmonizeDecimalPlaces(ref left, ref right);

            var quotient = Convert.ToDouble(left._units) / Convert.ToDouble(right._units);

            var result = new Money(left.CurrencyInfo, quotient);

            return result;
        }

        public static implicit operator Money(long value)
        {
            return new Money(value);
        }

        public static implicit operator Money(double value)
        {
            return new Money(CultureInfo.CurrentCulture, value);
        }

        public static implicit operator Money(decimal value)
        {
            return new Money(CultureInfo.CurrentCulture, value);
        }

        public static implicit operator long(Money value)
        {
            return (long) value.ScaleDownToDouble();
        }

        public static implicit operator double(Money value)
        {
            return value.ScaleDownToDouble();
        }

        public static implicit operator decimal(Money value)
        {
            return value.ScaleDownToDecimal();
        }
    }
}
