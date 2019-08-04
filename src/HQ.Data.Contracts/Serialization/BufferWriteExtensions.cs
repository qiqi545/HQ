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
using Microsoft.Extensions.Primitives;

namespace HQ.Data.Contracts.Serialization
{
	public static class BufferWriteExtensions
	{
		private static readonly Encoding Encoding = Encoding.UTF8;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteString(this ref Span<byte> buffer, ref int offset, StringValues value)
		{
			var stringValue = value.ToString(); // zero-alloc if single value

			if (value.Count <= 0)
			{
				buffer.MaybeResize(offset, 1);
				buffer.WriteBoolean(ref offset, value.Count > 0);
				return;
			}

			var charCount = stringValue.Length;
			var byteCount = Encoding.GetByteCount(stringValue);

			buffer.MaybeResize(offset, 1 + sizeof(int) + byteCount);
			buffer.WriteBoolean(ref offset, true);
			buffer.WriteInt32(ref offset, byteCount);

			unsafe
			{
				fixed (char* source = &stringValue.AsSpan().GetPinnableReference())
				fixed (byte* target = &buffer.Slice(offset, byteCount).GetPinnableReference())
					offset += Encoding.GetBytes(source, charCount, target, byteCount);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteBoolean(this ref Span<byte> buffer, ref int offset, bool value)
		{
			buffer[offset] = (byte) (value ? 1 : 0);
			offset++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteInt32(this ref Span<byte> buffer, ref int offset, int value)
		{
			unsafe
			{
				fixed (byte* b = &buffer.GetPinnableReference()) *(int*) (b + offset) = value;
			}

			offset += sizeof(int);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void MaybeResize(this ref Span<byte> buffer, long offset, int nextWriteLength)
		{
			var length = offset + nextWriteLength;
			if (buffer.Length >= length) return;

			var allocate = new byte[length];
			buffer.TryCopyTo(allocate);
			buffer = allocate.AsSpan();
		}
	}
}