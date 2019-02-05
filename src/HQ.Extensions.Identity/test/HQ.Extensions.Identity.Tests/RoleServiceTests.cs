using System;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Extensions.Identity.Models;
using HQ.Touchstone;
using HQ.Touchstone.Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Identity.Tests
{
    public abstract class RoleServiceTests : ServiceUnderTest
    {
        protected RoleServiceTests(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [Test]
        public async Task Can_create_role()
        {
            var service = serviceProvider.GetRequiredService<IRoleService<IdentityRoleExtended>>();
            var role = await service.CreateAsync(new CreateRoleModel
            {
                Name = "MyRole",
                ConcurrencyStamp = $"{Guid.NewGuid()}"
            });
            Assert.NotNull(role);
            Assert.NotNull(role.Data);

            await service.AddClaimAsync(role.Data, new Claim("Bob", "Loblaw"));

            var roles = await service.GetAsync();
            Assert.NotNull(roles.Data);
            Assert.Single(roles.Data);

            var roleClaims = await service.GetClaimsAsync(role.Data);
            Assert.NotNull(roleClaims);
            Assert.NotNull(roleClaims.Data);
            Assert.Single(roleClaims.Data);
            Assert.Equal("Bob", roleClaims.Data[0].Type);
            Assert.Equal("Loblaw", roleClaims.Data[0].Value);

            await service.AddClaimAsync(role.Data, new Claim("Bob2", "Loblaw2"));
            roleClaims = await service.GetClaimsAsync(role.Data);
            Assert.NotNull(roleClaims);
            Assert.NotNull(roleClaims.Data);
            Assert.Equal(2, roleClaims.Data.Count);

            await service.RemoveClaimAsync(role.Data, roleClaims.Data[0]);
            roleClaims = await service.GetClaimsAsync(role.Data);
            Assert.NotNull(roleClaims);
            Assert.NotNull(roleClaims.Data);
            Assert.Single(roleClaims.Data);

            await service.DeleteAsync(role.Data.Id);

            //var allClaims = await service.GetAllRoleClaimsAsync();
            //Assert.NotNull(allClaims.Data);
            //Assert.Empty(allClaims.Data);
        }
    }
}
