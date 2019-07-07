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
using System.Runtime.InteropServices;

namespace HQ.UI
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Value128
	{
		public ulong v1, v2;

		public static bool operator ==(Value128 a, Value128 b)
		{
			return a.v1 == b.v1 && a.v2 == b.v2;
		}

		public static bool operator !=(Value128 a, Value128 b)
		{
			return !(a == b);
		}

		public static Value128 operator ^(Value128 a, Value128 b)
		{
			return new Value128 {v1 = a.v1 ^ b.v1, v2 = a.v2 ^ b.v2};
		}

		public static implicit operator Value128(string id)
		{
			return new Value128
			{
				v1 = Convert.ToUInt64(id.Substring(0, 16), 16), v2 = Convert.ToUInt64(id.Substring(16, 16), 16)
			};
		}

		#region Object guff

		public override bool Equals(object obj)
		{
			return obj is Value128 && (Value128) obj == this;
		}

		public override int GetHashCode()
		{
			return (int) (v1 ^ v2);
		}

		public override string ToString()
		{
			return string.Format("{0:X8}{1:X8}", v1, v2);
		}

		#endregion
	}
}