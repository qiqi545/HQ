using System;
using System.Text;
using HQ.Cryptography.Internal;
using Sodium;

namespace HQ.Cryptography
{
    internal static class Strings
    {
        public static string BinToHex(byte[] buffer, StringSource source)
        {
            switch (source)
            {
                case StringSource.SystemNet:
                {
                    var sb = new StringBuilder(buffer.Length * 2);
                    foreach (var b in buffer)
                        sb.AppendFormat("{0:x2}", b);
                    return sb.ToString();
                }
                case StringSource.SodiumCore:
                    return Utilities.BinaryToHex(buffer, Utilities.HexFormat.None);
                case StringSource.SodiumCoreDirect:
                    return Utilities.BinaryToHex(buffer);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
    }
}
