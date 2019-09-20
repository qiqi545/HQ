using HQ.Extensions.Metrics.SplitTesting;
using Microsoft.AspNetCore.Http;
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

        public static IMetricsBuilder RegisterExperiment(this IMetricsBuilder builder, string name, string description, object[] alternatives = null, params string[] metrics)
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
            builder.Services.TryAddScoped<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.TryAddScoped<ICohortIdentifier, HttpRequestIdentifier>();
            return builder;
        }

        private static T GetOrAdd<T>(ExperimentKey name, T experiment) where T : Experiment
        {
            if (Experiments.Inner.TryGetValue(name, out var value))
                return (T)value;
            var added = Experiments.Inner.AddOrUpdate(name, experiment, (n, m) => m);
            return added == null ? experiment : (T)added;
        }
    }
}
