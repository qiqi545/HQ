#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HQ.Cadence.Internal
{
    /// <inheritdoc />
    /// <summary>
    ///     Properly serializes a metrics hash
    /// </summary>
    internal class MetricsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (!(value is IDictionary<MetricName, IMetric>)) return;

            var collection = (IDictionary<MetricName, IMetric>) value;
            var container = new List<MetricItem>(collection.Count);
            container.AddRange(collection.Select(item => new MetricItem {Name = item.Key.Name, Metric = item.Value}));
            var serialized = JsonSerializer.Serialize(container);

            writer.WriteRawValue(serialized);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<MetricName, IMetric>).IsAssignableFrom(objectType);
        }

        internal class MetricItem
        {
            public string Name { get; set; }
            public IMetric Metric { get; set; }
        }
    }
}
