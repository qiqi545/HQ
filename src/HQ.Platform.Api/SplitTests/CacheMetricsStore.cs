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

using System.Collections.Generic;
using System.Collections.Immutable;
using ActiveCaching;
using Metrics;
using IMetric = Metrics.IMetric;
using MetricName = Metrics.MetricName;

namespace HQ.Platform.Api.SplitTests
{
	public class CacheMetricsStore : ActiveStorage.IKeyValueStore<MetricName, IMetric>
	{
		private static readonly IImmutableDictionary<MetricName, IMetric> NoSample =
			ImmutableDictionary.Create<MetricName, IMetric>();

		private readonly ICache _cache;
		private readonly ActiveStorage.IKeyValueStore<MetricName, IMetric> _inner;

		public CacheMetricsStore(ICache cache)
		{
			_inner = new CacheKeyValueStore<MetricName, IMetric>(cache, Common.Constants.Categories.Metrics);
			_cache = cache;
		}

		public IImmutableDictionary<MetricName, IMetric> GetSample(MetricType excludeTypes = MetricType.None)
		{
			if (excludeTypes.HasFlagFast(MetricType.All))
			{
				return NoSample;
			}

			var filtered = new Dictionary<MetricName, IMetric>();
			foreach (var entry in _cache.Get<List<MetricName>>(Common.Constants.Categories.Metrics))
			{
				switch (entry.Class.Name)
				{
					case nameof(GaugeMetric) when excludeTypes.HasFlagFast(MetricType.Gauge):
					case nameof(CounterMetric) when excludeTypes.HasFlagFast(MetricType.Counter):
					case nameof(MeterMetric) when excludeTypes.HasFlagFast(MetricType.Meter):
					case nameof(HistogramMetric) when excludeTypes.HasFlagFast(MetricType.Histogram):
					case nameof(TimerMetric) when excludeTypes.HasFlagFast(MetricType.Timer):
						continue;
					default:
						filtered.Add(entry, _cache.Get<IMetric>(entry.CacheKey));
						break;
				}
			}

			return filtered.ToImmutableDictionary();
		}

		#region Implementation of IKeyValueStore<in MetricName,IMetric>

		public IMetric GetOrAdd(MetricName key, IMetric value)
		{
			return _inner.GetOrAdd(key, value);
		}

		public bool TryGetValue(MetricName key, out IMetric value)
		{
			return _inner.TryGetValue(key, out value);
		}

		public bool Contains(MetricName key)
		{
			return _inner.Contains(key);
		}

		public void AddOrUpdate<T>(MetricName key, T value) where T : IMetric
		{
			_inner.AddOrUpdate(key, value);
		}

		public IMetric this[MetricName key] => _inner[key];

		#endregion
	}
}