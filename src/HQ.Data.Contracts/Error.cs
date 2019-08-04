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
using System.Net;
using System.Runtime.Serialization;
using HQ.Common;

namespace HQ.Data.Contracts
{
	[DataContract]
	public class Error : IComparable<Error>, IComparable, IEquatable<Error>
	{
		public Error(long eventId, string message, HttpStatusCode statusCode, IEnumerable<Error> errors = null) : this(
			eventId, message, (short) statusCode, errors)
		{
		}

		public Error(long eventId, string message, short statusCode = (short) HttpStatusCode.InternalServerError,
			params Error[] errors) : this(eventId, message, statusCode, (IList<Error>) errors)
		{
			Message = message;
			StatusCode = statusCode;
			Errors = errors;
		}

		public Error(long eventId, string message, short statusCode = (short) HttpStatusCode.InternalServerError,
			IEnumerable<Error> errors = null)
		{
			EventId = eventId;
			Errors = errors?.MaybeList();
			StatusCode = statusCode;
			Message = message;
		}

		[DataMember] public short StatusCode { get; }

		[DataMember] public string Message { get; set; }

		[DataMember] public long EventId { get; }

		[DataMember] public IList<Error> Errors { get; set; }

		public int CompareTo(object obj)
		{
			if (ReferenceEquals(null, obj)) return 1;

			if (ReferenceEquals(this, obj)) return 0;

			return obj is Error other
				? CompareTo(other)
				: throw new ArgumentException($"Object must be of type {nameof(Error)}");
		}


		public int CompareTo(Error other)
		{
			if (ReferenceEquals(this, other)) return 0;

			if (ReferenceEquals(null, other)) return 1;

			var statusCodeComparison = StatusCode.CompareTo(other.StatusCode);
			if (statusCodeComparison != 0) return statusCodeComparison;

			var messageComparison = string.Compare(Message, other.Message, StringComparison.Ordinal);
			if (messageComparison != 0) return messageComparison;

			return EventId.CompareTo(other.EventId);
		}


		public bool Equals(Error other)
		{
			if (ReferenceEquals(null, other)) return false;

			if (ReferenceEquals(this, other)) return true;

			return StatusCode == other.StatusCode && string.Equals(Message, other.Message) && EventId == other.EventId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;

			if (ReferenceEquals(this, obj)) return true;

			if (obj.GetType() != GetType()) return false;

			return Equals((Error) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = StatusCode.GetHashCode();
				hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ EventId.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Error left, Error right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Error left, Error right)
		{
			return !Equals(left, right);
		}

		public static bool operator <(Error left, Error right)
		{
			return Comparer<Error>.Default.Compare(left, right) < 0;
		}

		public static bool operator >(Error left, Error right)
		{
			return Comparer<Error>.Default.Compare(left, right) > 0;
		}

		public static bool operator <=(Error left, Error right)
		{
			return Comparer<Error>.Default.Compare(left, right) <= 0;
		}

		public static bool operator >=(Error left, Error right)
		{
			return Comparer<Error>.Default.Compare(left, right) >= 0;
		}
	}
}