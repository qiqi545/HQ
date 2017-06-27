using System;
using Xunit;

namespace hq.compiler.tests
{
    /// <summary>
    /// These tests all require a reference to System.Runtime because outside of the runtime, all references
    /// are explicit (i.e. it isn't required in ASP.NET projects where the runtime is abstracted).
    /// 
    /// See: https://github.com/aspnet/dnx/issues/1653 
    /// </summary>
    public class HandlerFactoryTests : IClassFixture<HandlerFactoryFixture>
    {
        private readonly HandlerFactoryFixture _fixture;

        public HandlerFactoryTests(HandlerFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Can_build_default_handler()
        {
            var info = new HandlerInfo
            {
                Namespace = "hq",
                Function = "Execute",
                Entrypoint = "hq.Main",
                Code = @"
namespace hq
{ 
    public class Main
    { 
        public static string Execute()
        { 
            return ""Hello, World!"";
        }
    }
}"
            };

            var h = _fixture.Factory.BuildHandler(info);
            var r = (string) h.Invoke(null, new object[] {});

            Console.WriteLine(r); // Hello, World!
        }
    }
}
