using System;
using System.Threading.Tasks;

namespace hashing
{
    public class XxHashHashProvider : IHashProvider
    {
        public string Hash(long value)
        {
            var buffer = BitConverter.GetBytes(value);
            return XxHash.CalculateHash(buffer).ToString();
        }

        public Task<string> HashAsync(long value)
        {
            return Task.Run(() => Hash(value));
        }
    }
}