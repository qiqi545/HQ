using System;
using HQ.Cryptography.Internal;

namespace HQ.Cryptography
{
    public static class Crypto
    {
        public static byte[] RandomBytes(int length)
        {
            return Random.NextBytes(length, RandomSource.SodiumCore);
        }
    }
}
