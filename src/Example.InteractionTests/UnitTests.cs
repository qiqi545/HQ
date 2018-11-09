using System.Diagnostics;
using HQ.Touchstone;
using Microsoft.Extensions.Logging;

namespace Example.InteractionTests
{
    public class UnitTests : SystemUnderTest
    {
        [Test]
        public void Foo()
        {
            Trace.Write("This is a trace");

            this.LogCritical("This is a critical message");
        }
    }
}
