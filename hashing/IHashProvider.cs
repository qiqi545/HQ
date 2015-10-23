using System.Threading.Tasks;

namespace hashing
{
    public interface IHashProvider
    {
        string Hash(long value);
        string Hash(byte[] value);
        Task<string> HashAsync(byte[] value);
        Task<string> HashAsync(long value);
    }
}