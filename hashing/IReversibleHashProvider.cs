using System.Threading.Tasks;

namespace hashing
{
    public interface IReversibleHashProvider : IHashProvider
    {
        long Reverse(string value);
        Task<long> ReverseAsync(string value);
    }
}