using System.Threading.Tasks;
using HQ.Common;
using HQ.Platform.Security.AspNetCore;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Assertions;
using HQ.Test.Sdk.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Security.Tests
{
	public class CorsTests : SystemUnderTest
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddSecurityPolicies(o =>
			{
				o.Cors.Enabled = true;
				o.Cors.Origins = new [] { "https://approvedorigin.com" };

				o.Https.Enabled = false;
				o.Tokens.Enabled = false;
				o.Cookies.Enabled = false;
			});

            base.ConfigureServices(services);
		}

		public override void Configure(IApplicationBuilder app)
		{
			app.UseSecurityPolicies();
			base.Configure(app);
		}

		[Test]
		public async Task CorsTests_preflight_approved()
		{
			using (var client = CreateClient())
			{
                var response = await client.OptionsAsync("/", r =>
                {
	                r.Headers.TryAddWithoutValidation(Constants.HttpHeaders.Origin, "https://approvedorigin.com");
	                r.Headers.TryAddWithoutValidation(Constants.HttpHeaders.AccessControlRequestMethod, "GET");
	                r.Headers.TryAddWithoutValidation(Constants.HttpHeaders.AccessControlRequestHeaders, "Accept");
                });

                response.Should().BeOk();
                response.Should().HaveHeader(Constants.HttpHeaders.AccessControlAllowHeaders,
	                $"Missing pre-flight headers: {response.Headers}");
			}
		}

		[Test]
		public async Task CorsTests_preflight_not_approved()
		{
			using (var client = CreateClient())
			{
				var response = await client.OptionsAsync("/", r =>
				{
					r.Headers.TryAddWithoutValidation(Constants.HttpHeaders.Origin, "https://unapprovedorigin.com");
					r.Headers.TryAddWithoutValidation(Constants.HttpHeaders.AccessControlRequestMethod, "GET");
					r.Headers.TryAddWithoutValidation(Constants.HttpHeaders.AccessControlRequestHeaders, "Accept");
				});

				response.Should().BeOk();
				response.Should().NotHaveHeader(Constants.HttpHeaders.AccessControlAllowHeaders, "CORS policy approved an unapproved origin");
			}
		}
	}
}
