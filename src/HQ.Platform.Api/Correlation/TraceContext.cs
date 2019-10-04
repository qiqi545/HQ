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

using HQ.Extensions.Cryptography;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Correlation
{
	public struct TraceContext
	{
		public byte Version => 0;
		public byte[] TraceId { get; set; }
		public byte[] ParentId { get; set; }
		public TraceFlags Flags { get; set; }

		public static TraceContext Empty = new TraceContext();

		public static TraceContext New()
		{
			var context = new TraceContext
			{
				TraceId = Crypto.GetRandomBytes(16), ParentId = Crypto.GetRandomBytes(8), Flags = TraceFlags.None
			};
			return context;
		}

		public StringValues Header => $"00-{Crypto.BinToHex(TraceId)}-{Crypto.BinToHex(ParentId)}-{Flags:x}";
	}
}