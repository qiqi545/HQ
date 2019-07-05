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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using TimeZoneConverter;

namespace HQ.Common
{
    /// <summary>
    /// A <see cref="DateTimeOffset"/> replacement that doesn't lose time zone information, and normalizes to IANA time zone names.
    /// </summary>
    [DebuggerDisplay("{DisplayName}")]
    public struct DateTimeZone : IEquatable<DateTimeZone>, IEquatable<DateTimeOffset>, IComparable<DateTimeZone>, IComparable<DateTimeOffset>, IFormattable, IComparable, ISerializable, IDeserializationCallback
    {
        private readonly DateTimeOffset _instant;
        private readonly TimeZoneInfo _timeZone;

        public static DateTimeZone Now => new DateTimeZone(DateTimeOffset.UtcNow, TimeZoneInfo.Local);
        public static DateTimeZone UtcNow => new DateTimeZone(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);

        public DateTimeOffset DateTimeOffset => TimeZoneInfo.ConvertTime(_instant, _timeZone);
        
        /// <summary> Creates a <see cref="DateTimeZone"/> instance based on the most prevalent time zone associated with the provided instant's time zone offset </summary>
        public DateTimeZone(DateTimeOffset instant, string ianaOrWindowsTimeZoneId)
        {
            if(!TZConvert.TryGetTimeZoneInfo(ianaOrWindowsTimeZoneId, out _timeZone))
                throw new ArgumentException("The provided time zone identifier was not recognized.", nameof(ianaOrWindowsTimeZoneId));
            _instant = instant.ToUniversalTime();
        }

        /// <summary> Creates a <see cref="DateTimeZone"/> instance based on the most prevalent time zone associated with the provided instant's time zone offset </summary>
        internal DateTimeZone(DateTimeOffset instant, TimeZoneInfo timeZone)
        {
            _instant = instant.ToUniversalTime();
            _timeZone = timeZone;
        }

        public static implicit operator DateTimeOffset(DateTimeZone dateTimeZone) => dateTimeZone.DateTimeOffset;
        public static explicit operator DateTimeZone(DateTimeOffset dateTimeOffset)
        {
            if (dateTimeOffset.Offset == TimeSpan.Zero)
                throw new ArgumentException("Cannot cast a DateTimeOffset to DateTimeZone with no offset",
                    nameof(dateTimeOffset));

            foreach (var candidate in TZConvert.KnownWindowsTimeZoneIds.Select(TZConvert.GetTimeZoneInfo))
            {
                if (candidate.BaseUtcOffset != dateTimeOffset.Offset)
                    continue;

                return TZConvert.TryWindowsToIana(candidate.Id, CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                           out var iana) && TZConvert.TryGetTimeZoneInfo(iana, out var result)
                    ? new DateTimeZone(dateTimeOffset, result)
                    : new DateTimeZone(dateTimeOffset, candidate);
            }

            throw new NotSupportedException("Could not derive a time zone from the specified offset");
        }

        #region Equality

        public bool Equals(DateTimeZone other)
        {
            return DateTimeOffset.Equals(other.DateTimeOffset);
        }

        public int CompareTo(DateTimeOffset other)
        {
            return DateTimeOffset.CompareTo(other);
        }

        public bool Equals(DateTimeOffset other)
        {
            return DateTimeOffset.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is DateTimeZone other && Equals(other);
        }

        public override int GetHashCode()
        {
            return DateTimeOffset.GetHashCode();
        }

        public static bool operator ==(DateTimeZone left, DateTimeZone right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DateTimeZone left, DateTimeZone right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Comparison

        public int CompareTo(DateTimeZone other)
        {
            return DateTimeOffset.CompareTo(other.DateTimeOffset);
        }

        public int CompareTo(object obj)
        {
            return ReferenceEquals(null, obj) ? 1 :
                obj is DateTimeZone other ? CompareTo(other) :
                throw new ArgumentException($"Object must be of type {nameof(DateTimeZone)}");
        }

        public static bool operator <(DateTimeZone left, DateTimeZone right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(DateTimeZone left, DateTimeZone right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(DateTimeZone left, DateTimeZone right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(DateTimeZone left, DateTimeZone right)
        {
            return left.CompareTo(right) >= 0;
        }

        #endregion

        #region Formatting

        public override string ToString() => DisplayName;

        private string DisplayName => $"{DateTimeOffset} [{TimeZone}]";

        public void OnDeserialization(object sender)
        {
            ((IDeserializationCallback) DateTimeOffset).OnDeserialization(sender);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ((ISerializable) DateTimeOffset).GetObjectData(info, context);
        }

        private string TimeZone => TZConvert.WindowsToIana(_timeZone.StandardName);
        
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return DateTimeOffset.ToString(format, formatProvider);
        }

        #endregion

        #region DateTimeOffset Passthrough

        public DateTime UtcDateTime => DateTimeOffset.UtcDateTime;
        public TimeSpan Offset => DateTimeOffset.Offset;

        #endregion
    }
}
