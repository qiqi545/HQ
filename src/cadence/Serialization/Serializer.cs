using Newtonsoft.Json;

namespace cadence.Serialization
{
    internal class Serializer
    {
        private static readonly JsonSerializerSettings _settings;

        static Serializer()
        {
            _settings = new JsonSerializerSettings
                            {
                                DefaultValueHandling = DefaultValueHandling.Ignore,
                                NullValueHandling = NullValueHandling.Ignore,
                                ContractResolver = new JsonConventionResolver(),
                            };
            _settings.Converters.Add(new MetricsConverter());
        }

        public static string Serialize<T>(T entity)
        {
            return JsonConvert.SerializeObject(entity, Formatting.Indented, _settings);
        }
    }
}
