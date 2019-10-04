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
using System.Text;

namespace HQ.Data.Streaming.Fields
{
	[DebuggerDisplay("{" + nameof(DisplayName) + "}")]
	public readonly ref struct BooleanField
	{
		public bool Initialized => _buffer != null;

		public bool? Value =>
			Initialized ? !_encoding.TryParse(_buffer, out bool value) ? default(bool?) : value : default;

		public string RawValue => Initialized ? _encoding.GetString(_buffer) : default;
		public int Length => _buffer.Length;

		private readonly Encoding _encoding;
		private readonly ReadOnlySpan<byte> _buffer;

		public BooleanField(ReadOnlySpan<byte> buffer, Encoding encoding)
		{
			_buffer = buffer;
			_encoding = encoding;
		}

		public unsafe BooleanField(byte* start, int length, Encoding encoding)
		{
			try
			{
				_buffer = new ReadOnlySpan<byte>(start, length);
				_encoding = encoding;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string DisplayName =>
			$"{nameof(BooleanField).Replace("Field", string.Empty)}: {Value} ({RawValue ?? "<NULL>"}:{_encoding.BodyName})";
	}
}