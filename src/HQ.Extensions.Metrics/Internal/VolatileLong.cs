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

namespace HQ.Extensions.Metrics.Internal
{
	/// <summary>
	///     Provides support for volatile operations around a <see cref="long" /> value
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

		private VolatileLong(VolatileLong value) : this() => Set(value);

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