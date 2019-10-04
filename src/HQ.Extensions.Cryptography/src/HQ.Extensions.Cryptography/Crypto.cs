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
using HQ.Extensions.Cryptography.Internal;
using Sodium;
using Random = HQ.Extensions.Cryptography.Internal.Random;

namespace HQ.Extensions.Cryptography
{
	public static class Crypto
	{
		public static byte[] GetRandomBytes(int length)
		{
			return Random.NextBytes(length, RandomSource.SodiumCore);
		}

		public static void FillRandomBytes(Span<byte> buffer)
		{
			FillRandomBytes(buffer, buffer.Length);
		}

		public static void FillRandomBytes(Span<byte> buffer, int length)
		{
			Random.NextBytes(buffer, length, RandomSource.SodiumCore);
		}

		public static string BinToHex(ReadOnlySpan<byte> buffer)
		{
			return Strings.BinToHex(buffer, StringSource.SodiumCoreUnsafePooled);
		}

		public static string GetRandomString(int length)
		{
			return BinToHex(GetRandomBytes(length / 2));
		}

		public static bool ConstantTimeEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
		{
			return Utilities.Compare(left, right);
		}
	}
}