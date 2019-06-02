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

namespace HQ.Extensions.Cryptography.Internal
{
    internal static class BufferExtensions
    {
        public static ReadOnlySpan<byte> Concat(this byte[] buffer, byte[] append)
        {
            var result = new byte[checked(buffer.Length + append.Length)];
            Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
            Buffer.BlockCopy(append, 0, result, buffer.Length, append.Length);
            return result;
        }

        public static string ToHex(this byte[] input)
        {
            return Crypto.BinToHex(input);
        }
    }
}
