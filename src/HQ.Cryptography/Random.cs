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
using System.Security.Cryptography;
using HQ.Cryptography.Internal;
using NSec.Cryptography;
using Sodium;

namespace HQ.Cryptography
{
    internal static class Random
    {
        private static readonly RandomNumberGenerator SystemNetRandom = RandomNumberGenerator.Create();

        public static byte[] NextBytes(int length, RandomSource source)
        {
            var buffer = new byte[length];
            NextBytes(buffer, source);
            return buffer;
        }

        public static void NextBytes(byte[] buffer, RandomSource source)
        {
            switch (source)
            {
                case RandomSource.SystemNet:
                    SystemNetRandom.GetBytes(buffer);
                    break;
                case RandomSource.SodiumCore:
                    SodiumCore.FillRandomBytes(buffer);
                    break;
                case RandomSource.NSec:
                    RandomGenerator.Default.GenerateBytes(buffer);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public static void NextBytes(Span<byte> buffer, RandomSource source)
        {
            switch (source)
            {
                case RandomSource.SodiumCore:
                    SodiumCore.FillRandomBytes(buffer);
                    break;
                case RandomSource.NSec:
                    RandomGenerator.Default.GenerateBytes(buffer);
                    break;
                case RandomSource.SystemNet:
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
