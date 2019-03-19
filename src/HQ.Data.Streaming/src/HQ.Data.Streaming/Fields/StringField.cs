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
    public readonly ref struct StringField
    {
        public bool Initialized => _buffer != null;
        public string Value => RawValue;
        public string RawValue => Initialized ? _encoding.GetString(_buffer) : default;
        public int Length => _buffer.Length;

        private readonly unsafe byte* _start;
        private readonly int _length;
        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        public unsafe StringField(byte* start, int length, Encoding encoding)
        {
            _buffer = new ReadOnlySpan<byte>(start, length);
            _start = start;
            _length = length;
            _encoding = encoding;
        }

        public unsafe StringField AddLength(int length)
        {
            return new StringField(_start, _length + length, _encoding);
        }

        public string DisplayName =>
            $"{nameof(StringField).Replace("Field", string.Empty)}: {Value} ({RawValue ?? "<NULL>"}:{_encoding.BodyName})";
    }
}
