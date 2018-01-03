using System;
using System.IO;
using System.Reflection;
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
            var info = CreateDefaultHandler();
			var h = _fixture.Factory.BuildHandler(info);
            var r = (string) h.Invoke(null, new object[] {});
			Assert.Equal("Hello, World!", r);
        }

	    [Fact]
	    public void Can_roundtrip_assembly()
	    {
		    var tempPath = Path.GetTempFileName();

			var info = CreateDefaultHandler();
		    var a = _fixture.Factory.BuildAssemblyOnDisk(info, tempPath);
			Assert.NotNull(a);

		    string entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "hq"}.Main";
		    Type t = a?.GetType(entrypoint);
		    string function = info.Function ?? "Execute";
		    MethodInfo h = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);

			var r = (string)h.Invoke(null, new object[] { });
		    Assert.Equal("Hello, World!", r);

		    File.Delete(tempPath);
	    }

		private static HandlerInfo CreateDefaultHandler()
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
		    return info;
	    }
    }
}
