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
using System.Collections.Concurrent;
using System.Linq;
using HQ.Extensions.SplitTests.Internal;
using Metrics;

namespace HQ.Extensions.SplitTests
{
	public static class ExperimentExtensions
	{
		private const string Separator = "__";
		private const string Header = "__m__track__";

		static ExperimentExtensions() => SampleStore.Samples = new ConcurrentBag<Sample>();

		public static void TrackExperiment(this IMetricsHost host, string metric, int increment = 1)
		{
			if (increment <= 0) return;

			var now = DateTimeOffset.UtcNow;
			var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, now.Offset).ToUnixTime();

			var count = host.Counter(typeof(ExperimentExtensions), InternalMetric(metric, today)).Increment(increment);

			SampleStore.Samples.TryAdd(new Sample {Metric = metric, Value = count, SampledAt = now});

			// Track conversions for any registered experiments
			var experiments = Experiments.All.Values.Where(e => e.HasMetric(metric));
			foreach (var experiment in experiments)
			{
				experiment.Conversions[experiment.Alternative]++;
				experiment.CurrentParticipant.Converted = now;
			}
		}

		private static string InternalMetric(string tag, long timestamp)
		{
			return string.Concat(Header, tag, Separator, timestamp);
		}
	}
}