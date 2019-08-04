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
using System.Runtime.CompilerServices;
using System.Text;

namespace HQ.Data.Contracts.Serialization
{
	public static class BufferReadExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ReadString(this ReadOnlySpan<byte> buffer, int offset)
		{
			if (!ReadBoolean(buffer, offset)) return null;

			var length = ReadInt32(buffer, offset + 1);
			var sliced = buffer.Slice(offset + 1 + sizeof(int), length);

			unsafe
			{
				fixed (byte* b = sliced) return Encoding.UTF8.GetString(b, sliced.Length);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadBoolean(this ReadOnlySpan<byte> buffer, int offset)
		{
			return buffer[offset] != 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ReadInt32(this ReadOnlySpan<byte> buffer, int offset)
		{
			unsafe
			{
				fixed (byte* ptr = &buffer.GetPinnableReference()) return *(int*) (ptr + offset);
			}
		}
	}
}