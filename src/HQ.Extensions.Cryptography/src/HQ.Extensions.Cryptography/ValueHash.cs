using System;
using System.IO;
using TypeKitchen;
using WyHash;

namespace HQ.Extensions.Cryptography
{
    /// <summary>
    /// A non-cryptographic hash for comparing objects by value.
    /// </summary>
    public static class ValueHash
    {
        static ValueHash()
        {
            Seed = BitConverter.ToUInt64(new[]
            {
                (byte) 'H', (byte) 'Q', (byte) 'I', (byte) 'O',
                (byte) 'h', (byte) 'q', (byte) 'i', (byte) 'o'
            }, 0);
        }

        public static ulong Seed { get; set; }

        public static ulong ComputeHash(object instance)
        {
            var accessor = ReadAccessor.Create(instance, out var members);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var member in members)
                    {
                        WriteValue(accessor[instance, member.Name], bw);
                    }
                }

                return WyHash64.ComputeHash64(ms.GetBuffer(), Seed);
            }
        }

        private static void WriteValue(object value, BinaryWriter bw)
        {
            switch (value)
            {
                case string v:
                    bw.Write(v);
                    break;
                case byte v:
                    bw.Write(v);
                    break;
                case bool v:
                    bw.Write(v);
                    break;
                case short v:
                    bw.Write(v);
                    break;
                case ushort v:
                    bw.Write(v);
                    break;
                case int v:
                    bw.Write(v);
                    break;
                case uint v:
                    bw.Write(v);
                    break;
                case long v:
                    bw.Write(v);
                    break;
                case ulong v:
                    bw.Write(v);
                    break;
                case float v:
                    bw.Write(v);
                    break;
                case double v:
                    bw.Write(v);
                    break;
                case decimal v:
                    bw.Write(v);
                    break;
                case char v:
                    bw.Write(v);
                    break;
                case char[] v:
                    bw.Write(v);
                    break;
                case byte[] v:
                    bw.Write(v);
                    break;
                default:
                    bw.Write(ComputeHash(value));
                    break;
            }
        }
    }
}
