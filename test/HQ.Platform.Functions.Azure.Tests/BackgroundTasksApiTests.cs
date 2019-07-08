using System;
using System.Threading.Tasks;
using HQ.Platform.Functions.AspNetCore.Mvc;
using HQ.Platform.Security.AspNetCore;
using HQ.Platform.Security.Configuration;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Assertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Functions.Azure.Tests
{
	public class BackgroundTasksApiTests : SystemUnderTest
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			void ConfigureSecurityAction(SecurityOptions o)
			{
				o.Cors.Enabled = false;
				o.Https.Enabled = false;
				o.Tokens.Enabled = false;
				o.Cookies.Enabled = false;
			}

			services.AddSecurityPolicies(ConfigureSecurityAction);

			services.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddBackgroundTasksApi(ConfigureSecurityAction, o =>
				{
					o.Policy = null;
				});
		}

		public override void Configure(IApplicationBuilder app)
		{
			app.UseSecurityPolicies();
			app.UseMvc();
			base.Configure(app);
		}

		[Test]
		public async Task Get_all_background_tasks()
		{
			using (var client = CreateClient())
			{
                var response = await client.GetAsync("/ops/tasks");
                response.Should().BeOk();
			}
		}
	}
}
