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
using System.Text;
using System.Threading;

namespace HQ.Extensions.Types
{
    /// <summary>
    ///     Money is immutable and coupled to the <see cref="CurrencyInfo" /> it belongs
    ///     to at all times. In most cases, the code will attempt to determine
    ///     the correct <see cref="CurrencyInfo" /> on its own based on the culture of
    ///     the thread viewing the money, unless an explicit currency is provided.
    /// </summary>
    [Serializable]
    public partial struct Money : IComparable<Money>, IEquatable<Money>, IFormattable
    {
        private double? _override;
        private long _places;
        private long _units;

        public Money(long units) : this(CultureInfo.CurrentCulture, units)
        {
        }

        public Money(double value) : this(CultureInfo.CurrentCulture, value)
        {
        }

        public Money(decimal value) : this(CultureInfo.CurrentCulture, value)
        {
        }

        private Money(CurrencyInfo currencyInfo) : this()
        {
            CreatedDate = DateTime.UtcNow;
            CurrencyInfo = currencyInfo;
        }

        public Money(CurrencyInfo currencyInfo, long units) : this(currencyInfo)
        {
            _units = units;
        }

        public Money(CurrencyInfo currencyInfo, double value) : this(currencyInfo)
        {
            _units = ScaleUp(value);
        }

        public Money(CurrencyInfo currencyInfo, decimal value) : this(currencyInfo)
        {
            _units = ScaleUp(value);
        }

        public DateTime CreatedDate { get; }

        public CurrencyInfo CurrencyInfo { get; }

        public int CompareTo(Money other)
        {
            return other._units.CompareTo(_units);
        }

        public bool Equals(Money other)
        {
            return other == this;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            var value = ScaleDownToDouble();

            return value.ToString(format, formatProvider);
        }

        public bool Equals(double other)
        {
            return other == ScaleDownToDouble();
        }

        public bool Equals(long other)
        {
            return other == ScaleUp(ScaleDownToDouble());
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return other.GetType() == typeof(Money) &&
                   Equals((Money) other);
        }

        public override int GetHashCode()
        {
            return _units.GetHashCode();
        }

        private double ScaleDownToDouble()
        {
            if (_override.HasValue)
            {
                return _override.Value;
            }

            var numberFormat = CurrencyInfo.DisplayCulture.NumberFormat;

            var scalingFactor = Math.Pow(10, _places);

            var scaled = _units / scalingFactor;

            var rounded = Math.Round(scaled, numberFormat.CurrencyDecimalDigits);

            return rounded;
        }

        private decimal ScaleDownToDecimal()
        {
            var numberFormat = CurrencyInfo.DisplayCulture.NumberFormat;

            var scalingFactor = Convert.ToDecimal(Math.Pow(10, _places));

            var scaled = _units / scalingFactor;

            var rounded = Math.Round(scaled, numberFormat.CurrencyDecimalDigits);

            return rounded;
        }

        private long ScaleUp(double value)
        {
            if (double.IsInfinity(value))
            {
                _override = value;
                return long.MaxValue;
            }

            var places = CountDecimalPlaces(value);

            var scalingFactor = Math.Pow(10, places);

            var scaled = Convert.ToInt64(value * scalingFactor);

            _places = places;

            return scaled;
        }

        private long ScaleUp(decimal value)
        {
            var places = CountDecimalPlaces(value);

            var scalingFactor = Convert.ToDecimal(Math.Pow(10, places));

            var scaled = Convert.ToInt64(value * scalingFactor);

            _places = places;

            return scaled;
        }

        private static void EnsureSameCurrency(Money left, Money right)
        {
            if (left.CurrencyInfo != right.CurrencyInfo)
            {
                throw new ArithmeticException("The currency of both arguments must match to perform this operation.");
            }
        }

        private static void HarmonizeDecimalPlaces(ref Money left, ref Money right)
        {
            var scaleFactor = Math.Abs(right._places - left._places);

            if (right._places > left._places)
            {
                left._places += scaleFactor;

                left._units *= (long) Math.Pow(10, scaleFactor);
            }

            if (right._places >= left._places)
            {
                return;
            }

            right._places += scaleFactor;

            right._units *= (long) Math.Pow(10, scaleFactor);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var displayCulture = Thread.CurrentThread.CurrentCulture;
            return DisplayIn(displayCulture, false);
        }

        /// <summary>
        ///     Displays the current instance as it would appear in the native culture,
        ///     no matter 'where' the context thread is running.
        /// </summary>
        /// <returns></returns>
        public string DisplayNative()
        {
            return ToString("c", CurrencyInfo.DisplayCulture.NumberFormat);
        }

        /// <summary>
        ///     Displays the current instance as it would appear in a specified culture.
        /// </summary>
        /// <param name="displayCulture">The display culture.</param>
        /// <returns></returns>
        public string DisplayIn(CultureInfo displayCulture)
        {
            return DisplayIn(displayCulture, true);
        }

        /// <summary>
        ///     Displays the value of this instance in a non-native culture, preserving
        ///     the characteristics of the native <see cref="CurrencyInfo" /> but respecting
        ///     target cultural formatting.
        /// </summary>
        /// <param name="displayCulture">The culture to display this money in</param>
        /// <param name="disambiguateMatchingSymbol">
        ///     If <code>true</code>, if the native culture uses the same currency symbol as
        ///     the display culture, the ISO currency code is appended to the value to help differentiate the native currency.
        /// </param>
        /// <returns>A value representing this instance in another culture</returns>
        public string DisplayIn(CultureInfo displayCulture, bool disambiguateMatchingSymbol)
        {
            var sb = new StringBuilder();

            var nativeCulture = CurrencyInfo.DisplayCulture;
            if (displayCulture == nativeCulture)
            {
                return nativeCulture.ToString();
            }

            var nativeNumberFormat = nativeCulture.NumberFormat;
            nativeNumberFormat = (NumberFormatInfo) nativeNumberFormat.Clone();

            var displayNumberFormat = displayCulture.NumberFormat;
            nativeNumberFormat.CurrencyGroupSeparator = displayNumberFormat.CurrencyGroupSeparator;
            nativeNumberFormat.CurrencyDecimalSeparator = displayNumberFormat.CurrencyDecimalSeparator;

            sb.Append(ToString("c", nativeNumberFormat));

            // If the currency symbol of the display culture matches this money, add the code
            if (disambiguateMatchingSymbol &&
                nativeNumberFormat.CurrencySymbol.Equals(displayNumberFormat.CurrencySymbol))
            {
                var currencyCode = new RegionInfo(nativeCulture.LCID).ISOCurrencySymbol;
                sb.Append(" ").Append(currencyCode);
            }

            return sb.ToString();
        }

        private static int CountDecimalPlaces(double input)
        {
            var value = input.ToString();

            var places = value.Substring(value.IndexOf('.') + 1).Length;

            return places;
        }

        private static int CountDecimalPlaces(decimal input)
        {
            var value = input.ToString();

            var places = value.Substring(value.IndexOf('.') + 1).Length;

            return places;
        }
    }
}
