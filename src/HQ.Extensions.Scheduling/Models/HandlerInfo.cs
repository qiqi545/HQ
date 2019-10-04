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

namespace HQ.Extensions.Scheduling.Models
{
	public class HandlerInfo : IEquatable<HandlerInfo>
	{
		public HandlerInfo( /* Required for serialization */) { }

		public HandlerInfo(string @namespace, string entrypoint)
		{
			Namespace = @namespace;
			Entrypoint = entrypoint;
		}

		public string Namespace { get; set; }
		public string Entrypoint { get; set; }

		#region Equality

		public bool Equals(HandlerInfo other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return string.Equals(Namespace, other.Namespace) && string.Equals(Entrypoint, other.Entrypoint);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((HandlerInfo) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Namespace != null ? Namespace.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ (Entrypoint != null ? Entrypoint.GetHashCode() : 0);
				hashCode = hashCode * 397;
				return hashCode;
			}
		}

		public static bool operator ==(HandlerInfo left, HandlerInfo right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(HandlerInfo left, HandlerInfo right)
		{
			return !Equals(left, right);
		}

		private sealed class NamespaceEntrypointInstanceEqualityComparer : IEqualityComparer<HandlerInfo>
		{
			public bool Equals(HandlerInfo x, HandlerInfo y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				return string.Equals(x.Namespace, y.Namespace) && string.Equals(x.Entrypoint, y.Entrypoint);
			}

			public int GetHashCode(HandlerInfo obj)
			{
				unchecked
				{
					var hashCode = obj.Namespace != null ? obj.Namespace.GetHashCode() : 0;
					hashCode = (hashCode * 397) ^ (obj.Entrypoint != null ? obj.Entrypoint.GetHashCode() : 0);
					hashCode = hashCode * 397;
					return hashCode;
				}
			}
		}

		public static IEqualityComparer<HandlerInfo> NamespaceEntrypointInstanceComparer { get; } =
			new NamespaceEntrypointInstanceEqualityComparer();

		#endregion
	}
}