using System.Threading.Tasks;
using hashing;

namespace gadfly.Core
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