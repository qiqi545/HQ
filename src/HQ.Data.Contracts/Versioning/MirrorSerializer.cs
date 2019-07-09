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
using System.Buffers;
using System.IO;
using Microsoft.Extensions.Primitives;
using TypeKitchen;

namespace HQ.Data.Contracts.Versioning
{
    public static class MirrorSerializer
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();

        public static void Serialize<T>(this T subject, Stream stream)
        {
            var accessor = ReadAccessor.Create(typeof(T), AccessorMemberTypes.Properties, out var members);

            var buffer = Pool.Rent(4096);
            try
            {
                var offset = 0;
                var span = buffer.AsSpan();

                foreach (var member in members)
                {
                    if (!member.CanWrite)
                        continue;

                    var value = accessor[subject, member.Name];

                    switch (value)
                    {
                        case string v:
                            span.WriteString(ref offset, v);
                            break;
                        case StringValues v:
                            span.WriteString(ref offset, v);
                            break;
                        case int v:
                            span.WriteInt32(ref offset, v);
                            break;
                        case bool v:
                            span.WriteBoolean(ref offset, v);
                            break;
                    }
                }

                stream.Write(buffer, 0, offset);
            }
            finally
            {
                Pool.Return(buffer);
            }
        }
    }
}
