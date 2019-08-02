using System.Text;
using Newtonsoft.Json;

namespace HQ.Extensions.Caching
{
    internal class JsonCacheSerializer : ICacheSerializer
    {
        public byte[] Serialize<T>(T value)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
        }

        public T Deserialize<T>(byte[] bytes)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
        }
    }
}
