// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Newtonsoft.Json;

namespace HQ.Cadence.Internal
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
