using System.Threading.Tasks;

namespace HQ.Platform.Security.Messaging
{
    public interface IKeygenStore
    {
        Task<byte[]> AcquireKeyAsync(KeyType keyType);
        Task<byte[]> AcquireNonceAsync(byte[] key);
    }
}
