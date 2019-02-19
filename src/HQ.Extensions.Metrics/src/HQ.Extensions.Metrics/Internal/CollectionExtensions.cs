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

namespace HQ.Extensions.Metrics.Internal
{
    internal static class CollectionExtensions
    {
        public static IDictionary<string, IDictionary<string, IMetric>> Sort(
            this IReadOnlyDictionary<MetricName, IMetric> metrics)
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
