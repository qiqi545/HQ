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

using HQ.Extensions.Metrics.SplitTesting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HQ.Extensions.Metrics.AspNetCore.SplitTesting
{
	public static class Add
	{
		public static IServiceCollection AddSplitTesting(this IServiceCollection services)
		{
			return services.AddMetrics(builder =>
			{
				builder.AddSplitTesting();
			});
		}

		public static IMetricsBuilder RegisterExperiment(this IMetricsBuilder builder, string name, string description,
			object[] alternatives = null, params string[] metrics)
		{
			builder.AddSplitTesting();
			builder.Services.AddScoped(r =>
			{
				var identifier = r.GetRequiredService<ICohortIdentifier>();
				var experiment = new Experiment(identifier, name, description, alternatives, metrics);
				return GetOrAdd(new ExperimentKey(name), experiment);
			});
			return builder;
		}

		public static IMetricsBuilder AddSplitTesting(this IMetricsBuilder builder)
		{
			builder.Services.AddHttpContextAccessor();
			builder.Services.TryAddScoped<ICohortIdentifier, HttpRequestIdentifier>();
			return builder;
		}

		private static T GetOrAdd<T>(ExperimentKey name, T experiment) where T : Experiment
		{
			if (Experiments.Inner.TryGetValue(name, out var value))
				return (T) value;
			var added = Experiments.Inner.AddOrUpdate(name, experiment, (n, m) => m);
			return added == null ? experiment : (T) added;
		}
	}
}