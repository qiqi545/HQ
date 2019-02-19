using HQ.Cryptography.Internal;

namespace HQ.Cryptography.Tests
{
    public class RandomStringFixture
    {
        public RandomStringFixture()
        {
            Value = Strings.BinToHex(Crypto.RandomBytes(32), StringSource.SystemNet);
        }

        public string Value { get; set; }
    }
}
