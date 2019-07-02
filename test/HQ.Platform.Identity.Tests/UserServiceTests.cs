using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.Configuration;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Extensions;
using HQ.Test.Sdk.Xunit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Tests
{
    public abstract class UserServiceTests : ServiceUnderTest
    {
        protected UserServiceTests(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [Test]
        public async Task Can_create_user()
        {
            var tenantService = ServiceProvider.GetRequiredService<ITenantService<IdentityTenant>>();
            var tenant = await tenantService.CreateAsync(new CreateTenantModel
            {
                Name = "MyTenant",
                ConcurrencyStamp = $"{Guid.NewGuid()}"
            });
            Assert.NotNull(tenant);
            Assert.NotNull(tenant.Data);

            var security = ServiceProvider.GetRequiredService<IOptions<SecurityOptions>>();
            this.SetAsAuthenticated(claims: new Claim(security.Value.Claims.TenantIdClaim, tenant.Data.Id));

            var userService = ServiceProvider.GetRequiredService<IUserService<IdentityUserExtended>>();
            var user = await userService.CreateAsync(new CreateUserModel
            {
                UserName = "MyUserName",
                Password = "MyPassword"
            });
            Assert.NotNull(user);
            Assert.NotNull(user.Data);
        }
    }
}
