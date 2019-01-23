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
using System.Text;

namespace HQ.Data.Streaming.Fields
{
    public readonly ref struct CharBooleanField
    {
        public bool? Value => TryConvertValue();

        private bool? TryConvertValue()
        {
            var @char = !char.TryParse(_encoding.GetString(_buffer), out var value) ? default(char?) : value;
            if (!@char.HasValue)
                return null;
            if (_true.Contains(@char.Value))
                return true;
            if (_false.Contains(@char.Value))
                return false;
            return null;
        }

        public string RawValue => _encoding.GetString(_buffer);

        private readonly Encoding _encoding;
        private readonly ReadOnlySpan<byte> _buffer;

        private readonly HashSet<char> _true;
        private readonly HashSet<char> _false;

        public CharBooleanField(HashSet<char> @true, HashSet<char> @false, ReadOnlySpan<byte> buffer, Encoding encoding)
        {
            _true = @true;
            _false = @false;
            _buffer = buffer;
            _encoding = encoding;
        }

        public unsafe CharBooleanField(HashSet<char> @true, HashSet<char> @false, byte* start, int length,
            Encoding encoding)
        {
            _true = @true;
            _false = @false;
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
    }
}
