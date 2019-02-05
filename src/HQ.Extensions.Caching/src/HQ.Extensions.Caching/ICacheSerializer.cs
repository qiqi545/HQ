namespace HQ.Extensions.Caching
{
    public interface ICacheSerializer
    {
        byte[] Serialize<T>(T value);
        T Deserialize<T>(byte[] bytes);
    }
}
