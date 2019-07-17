using System.Collections.Generic;
using System.Threading.Tasks;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Api.Functions.AspNetCore.Mvc;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Models;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Assertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.InteractionTests
{
	public class BackgroundTasksApiTests : SystemUnderTest
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddBackgroundTasksApi();
		}

		public override void Configure(IApplicationBuilder app)
		{
			app.UseAuthentication();
			app.UseBackgroundTasksApi();
		}

		[Test]
		public async Task Unauthorized_when_policy_set_and_no_token_provided()
		{
			await Act("/ops/tasks", response =>
			{
				response.Should().BeUnauthorized();
			});
		}

		[Test]
		public async Task Options_discovers_available_tasks()
		{
			Arrange(RemoveAuthorizationPolicy);

			await Act<BackgroundTaskOptionsView>("/ops/tasks", x =>
			{
				Assert.NotEmpty(x.TaskTypes);
			});
		}

		[Test]
		public async Task Get_no_background_tasks_when_none_added()
		{
			Arrange(RemoveAuthorizationPolicy);

			await Act<IEnumerable<BackgroundTask>>("/ops/tasks", x =>
			{
				Assert.Empty(x);
			});
		}

		private static void RemoveAuthorizationPolicy(IServiceCollection services)
		{
			services.Configure<BackgroundTaskOptions>(o => { o.Policy = null; });
		}
	}
}
