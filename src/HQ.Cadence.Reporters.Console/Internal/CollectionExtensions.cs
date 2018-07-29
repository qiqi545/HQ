// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;

namespace HQ.Cadence.Reporters.Console.Internal
{
	internal static class CollectionExtensions
	{
		public static IDictionary<string, IDictionary<string, IMetric>> Sort(this IReadOnlyDictionary<MetricName, IMetric> metrics)
		{
			var sortedMetrics = new SortedDictionary<string, IDictionary<string, IMetric>>();

			foreach (var entry in metrics)
			{
				var className = entry.Key.Class.Name;
				IDictionary<string, IMetric> submetrics;
				if (!sortedMetrics.ContainsKey(className))
				{
					submetrics = new SortedDictionary<string, IMetric>();
					sortedMetrics.Add(className, submetrics);
				}
				else
				{
					submetrics = sortedMetrics[className];
				}
				submetrics.Add(entry.Key.Name, entry.Value);
			}
			return sortedMetrics;
		}
	}
}
