using System.Diagnostics;
using System.Threading.Tasks;
using HQ.Touchstone;
using HQ.Touchstone.Assertions;
using Microsoft.Extensions.Logging;

namespace Example.InteractionTests
{
    public class InteractionTests : SystemUnderTest<Startup>
    {
        [Test]
        public async Task PassingTest()
        {
            Trace.WriteLine("This is a trace");

            this.LogInformation("This is a log message");

            using (var client = CreateClient())
            {
                var response = await client.GetAsync("/api/values");

                response.Should().BeOk();
                response.Should().HaveBody();
            }
        }
    }
}
