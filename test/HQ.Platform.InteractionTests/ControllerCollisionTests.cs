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

using System.Threading.Tasks;
using HQ.Platform.InteractionTests.Fakes;
using HQ.Platform.Operations;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Assertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.InteractionTests
{
	public class ControllerCollisionTests : SystemUnderTest
	{
		public override void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddApplicationPart(typeof(ConfigurationController).Assembly);
		}

		public override void Configure(IApplicationBuilder app)
		{
			app.UseMvc();
		}

		[Test]
		public async Task Controller_is_visible_with_no_collision()
		{
			await Act("/gravy", response =>
			{
				response.Should().BeOk();
			});
		}

		[Test]
		public async Task Controller_is_responsive_with_multiple_registrations_of_the_same_name()
		{
			Arrange(services =>
			{
				services.AddConfigurationApi();
			});
			
			await Act("/gravy", response =>
			{
				response.Should().BeOk();
			});
		}
	}
}