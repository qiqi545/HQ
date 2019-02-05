namespace HQ.Extensions.Caching
{
    internal class JsonCacheSerializer : ICacheSerializer
    {
        public byte[] Serialize<T>(T value)
        {
            return Utf8Json.JsonSerializer.Serialize(value);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return Utf8Json.JsonSerializer.Deserialize<T>(bytes);
        }
    }
}
