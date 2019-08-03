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
using HQ.Data.Contracts.Versioning;

namespace HQ.Data.Contracts.Tests.Versioning
{
    namespace V1
    {
        public ref struct PersonMirror
        {
            private readonly ReadOnlySpan<byte> _buffer;

            public string Name => _buffer.ReadString(0);

            public PersonMirror(ReadOnlySpan<byte> buffer)
            {
                _buffer = buffer;
            }
        }
    }

    namespace V2
    {
        public ref struct PersonMirror
        {
            private readonly ReadOnlySpan<byte> _buffer;

            public string FirstName => _buffer.ReadString(0);

            public string LastName
            {
                get
                {
                    var offset = 1;
                    if (_buffer.ReadBoolean(0))
                    {
                        offset += sizeof(int) + _buffer.ReadInt32(1);
                    }

                    return _buffer.ReadString(offset);
                }
            }

            public PersonMirror(ReadOnlySpan<byte> buffer)
            {
                _buffer = buffer;
            }
        }
    }
}
