// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Security.Cryptography;

namespace HQ.Cadence.Support
{
    /// <summary>
    /// Provides statistically relevant random number generation
    /// </summary>
    public class Random
    {
        private static readonly RandomNumberGenerator _random;

        static Random()
        {
            _random = RandomNumberGenerator.Create();   
        }
        
        public static long NextLong()
        {
            var buffer = new byte[sizeof(long)];
            _random.GetBytes(buffer);
            var value = BitConverter.ToInt64(buffer, 0);
            return value;
        }
    }
}
