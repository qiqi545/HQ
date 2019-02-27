using System;
using System.Threading.Tasks;
using Sodium;

namespace HQ.Platform.Security.Messaging
{
    public class UnsafeKeygenStore : IKeygenStore
    {
        public Task<byte[]> AcquireKeyAsync()
        {
            return Task.FromResult(OneTimeAuth.GenerateKey());
        }

        public Task<byte[]> AcquireKeyAsync(KeyType keyType)
        {
            switch (keyType)
            {
                case KeyType.OneTimePassword:
                    return Task.FromResult(OneTimeAuth.GenerateKey());
                case KeyType.PrivateKey:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(keyType), keyType, null);
            }
        }

        public Task<byte[]> AcquireNonceAsync(byte[] key)
        {
            return Task.FromResult(SecretBox.GenerateNonce());
        }
    }
}
