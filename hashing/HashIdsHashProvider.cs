using System.Threading.Tasks;

namespace hashing
{
    public class HashidsHashProvider : IReversibleHashProvider
    {
        private readonly Hashids _hashIds;

        public HashidsHashProvider()
        {
            _hashIds = new Hashids("HashIdsHashProvider");
        }

        public string Hash(long value)
        {
            return _hashIds.EncodeLong(value);
        }

        public string Hash(byte[] value)
        {
            int[] buffer = new int[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                buffer[i] = value[i];
            }
            return _hashIds.Encode(buffer);
        }

        public Task<string> HashAsync(byte[] value)
        {
            return Task.Run(() => Hash(value));
        }

        public Task<string> HashAsync(long value)
        {
            return Task.Run(() => Hash(value));
        }

        public long Reverse(string value)
        {
            return _hashIds.DecodeLong(value)[0];
        }

        public Task<long> ReverseAsync(string value)
        {
            return Task.Run(() => Reverse(value));
        }
    }
}