// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace HQ.Cadence.Internal
{
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