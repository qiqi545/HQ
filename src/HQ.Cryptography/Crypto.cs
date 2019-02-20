using System;
using HQ.Cryptography.Internal;
using Random = HQ.Cryptography.Internal.Random;

namespace HQ.Cryptography
{
    public static class Crypto
    {
        public static byte[] GetRandomBytes(int length)
        {
            return Random.NextBytes(length, RandomSource.SodiumCore);
        }

        public static void FillRandomBytes(Span<byte> buffer)
        {
            FillRandomBytes(buffer, buffer.Length);
        }

        public static void FillRandomBytes(Span<byte> buffer, int length)
        {
            Random.NextBytes(buffer, length, RandomSource.SodiumCore);
        }

        public static string BinToHex(ReadOnlySpan<byte> buffer)
        {
            return Strings.BinToHex(buffer, StringSource.SodiumCoreUnsafePooled);
        }

        public static string GetRandomString(int length)
        {
            return BinToHex(GetRandomBytes(length / 2));
        }
    }
}
