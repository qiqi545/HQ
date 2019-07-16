using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.Configuration;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Tests
{
    public abstract class RoleServiceTests : ServiceUnderTest
    {
        protected RoleServiceTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            var security = ServiceProvider.GetRequiredService<IOptions<SecurityOptions>>();
            this.SetAsAuthenticated(claims: new[]
            {
                new Claim(security.Value.Claims.TenantIdClaim, Guid.NewGuid().ToString()),
                new Claim(security.Value.Claims.ApplicationIdClaim, Guid.NewGuid().ToString())
            });
        }

        [Test]
        public async Task Can_create_role()
        {
            var userService = ServiceProvider.GetRequiredService<IUserService<IdentityUserExtended>>();
            var user = await userService.CreateAsync(new CreateUserModel
            {
                UserName = "MyUserName",
                Password = "MyPassword"
            });
            Assert.True(user.Succeeded);
            
            var roleService = ServiceProvider.GetRequiredService<IRoleService<IdentityRoleExtended>>();
            var role = await roleService.CreateAsync(new CreateRoleModel
            {
                Name = "MyRole",
                ConcurrencyStamp = $"{Guid.NewGuid()}"
            });
            Assert.NotNull(role);
            Assert.NotNull(role.Data);

            await roleService.AddClaimAsync(role.Data, new Claim("Bob", "Loblaw"));

            var roles = await roleService.GetAsync();
            Assert.NotNull(roles.Data);
            Assert.Single(roles.Data);

            var roleClaims = await roleService.GetClaimsAsync(role.Data);
            Assert.NotNull(roleClaims);
            Assert.NotNull(roleClaims.Data);
            Assert.Single(roleClaims.Data);
            Assert.Equal("Bob", roleClaims.Data[0].Type);
            Assert.Equal("Loblaw", roleClaims.Data[0].Value);

            await roleService.AddClaimAsync(role.Data, new Claim("Bob2", "Loblaw2"));
            roleClaims = await roleService.GetClaimsAsync(role.Data);
            Assert.NotNull(roleClaims);
            Assert.NotNull(roleClaims.Data);
            Assert.Equal(2, roleClaims.Data.Count);

            await roleService.RemoveClaimAsync(role.Data, roleClaims.Data[0]);
            roleClaims = await roleService.GetClaimsAsync(role.Data);
            Assert.NotNull(roleClaims);
            Assert.NotNull(roleClaims.Data);
            Assert.Single(roleClaims.Data);

            await roleService.DeleteAsync(role.Data.Id);

            //var allClaims = await service.GetAllRoleClaimsAsync();
            //Assert.NotNull(allClaims.Data);
            //Assert.Empty(allClaims.Data);
        }
    }
}
