using System;
using System.Threading.Tasks;
using HQ.Cohort.Models;
using HQ.Touchstone;
using HQ.Touchstone.Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Cohort.Tests
{
    public abstract class UserServiceTests : ServiceUnderTest
    {
        protected UserServiceTests(IServiceProvider serviceProvider) : base(serviceProvider) { }

        [Test]
        public async Task Can_create_user()
        {
            var service = serviceProvider.GetRequiredService<IUserService<IdentityUserExtended>>();
            var role = await service.CreateAsync(new CreateUserModel
            {
                UserName = "MyUserName",
                ConcurrencyStamp = $"{Guid.NewGuid()}"
            });
            Assert.NotNull(role);
            Assert.NotNull(role.Data);
        }
    }
}
