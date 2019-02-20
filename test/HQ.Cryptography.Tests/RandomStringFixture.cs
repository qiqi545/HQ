using HQ.Cryptography.Internal;

namespace HQ.Cryptography.Tests
{
    public class RandomStringFixture
    {
        public RandomStringFixture()
        {
            Value = Crypto.GetRandomString(64);
        }

        public string Value { get; set; }
    }
}
