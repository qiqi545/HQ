using System.Diagnostics;
using System.Threading.Tasks;
using HQ.Touchstone;
using HQ.Touchstone.Assertions;
using HQ.Touchstone.Xunit;

namespace Example.InteractionTests
{
    public class InteractionTests : SystemUnderTest<Startup>
    {
        [Test]
        public async Task FailingTest()
        {
            Trace.WriteLine("This is a trace");

            LogInformation("This is a log message");

            using (var client = CreateClient())
            {
                var response = await client.GetAsync("/api/values");

                response.Should().BeOk();
                response.Should().HaveBody();
            }
        }
    }
}
