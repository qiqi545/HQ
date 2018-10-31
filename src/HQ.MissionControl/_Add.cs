using HQ.MissionControl.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.MissionControl
{
	public static class Add
	{
		public static IServiceCollection AddDevOpsApi(this IServiceCollection services, IConfiguration config)
		{
			services.Configure<DevOpsOptions>(config);
			services.AddSingleton(config);
			return services;
		}
	}
}
