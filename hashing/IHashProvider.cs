using System.Threading.Tasks;

namespace hashing
{
    public interface IHashProvider
    {
        string Hash(long value);
        Task<string> HashAsync(long value);
    }
}