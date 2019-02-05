namespace HQ.Extensions.Caching
{
    public interface IClearable : ICache
    {
        void Clear();
    }
}