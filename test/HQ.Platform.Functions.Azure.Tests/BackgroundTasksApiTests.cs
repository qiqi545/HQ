using System.Collections.Generic;
using System.Threading.Tasks;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Functions.AspNetCore.Mvc;
using HQ.Platform.Functions.AspNetCore.Mvc.Models;
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
			services.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddBackgroundTasksApi(o =>
				{
					o.Cors.Enabled = false;
					o.Https.Enabled = false;
					o.Tokens.Enabled = false;
					o.Cookies.Enabled = false;
				}, o =>
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
		public async Task Options_discovers_available_tasks()
		{
			await Act<BackgroundTaskOptionsView>("/ops/tasks", view =>
			{
				Assert.NotEmpty(view.TaskTypes);
			});
		}

		[Test]
		public async Task Get_no_background_tasks_when_none_added()
		{
			await Act<IEnumerable<BackgroundTask>>("/ops/tasks", tasks =>
			{
				Assert.Empty(tasks);
			});
		}
	}
}
