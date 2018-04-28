using System.Diagnostics;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace hq.compiler.tests
{
    /// <inheritdoc />
    /// <summary>
    /// These tests all require a reference to System.Runtime because outside of the runtime, all references
    /// are explicit (i.e. it isn't required in ASP.NET projects where the runtime is abstracted).
    /// See: https://github.com/aspnet/dnx/issues/1653 
    /// </summary>
    public class HandlerFactoryTests : IClassFixture<HandlerFactoryFixture>
    {
        private readonly HandlerFactoryFixture _fixture;
	    private readonly ITestOutputHelper _console;

	    public HandlerFactoryTests(HandlerFactoryFixture fixture, ITestOutputHelper console)
	    {
		    _fixture = fixture;
		    _console = console;
	    }

        [Fact]
        public void Can_build_default_handler()
        {
            var info = CreateDefaultHandler();
			var h = _fixture.Factory.BuildHandler(info);
            var r = (string) h.DynamicInvoke();
			Assert.Equal("Hello, World!", r);
        }

	    [Fact]
	    public void Bench_all_strategies_vs_methodinfo()
	    {
		    var info = CreateDefaultHandler();

		    const int trials = 1000000;
		    var sw = Stopwatch.StartNew();

		    var noArgs = new object[] { };
		    var m = _fixture.Factory.BuildHandlerDirect(info);
		    sw.Restart();
		    for (var i = 0; i < trials; i++)
			    Assert.Equal("Hello, World!", m.Invoke(null, noArgs));
		    _console.WriteLine($"MethodInfo (direct) {trials}x took {sw.Elapsed}");

		    BenchStrategy(sw, trials, HandlerFactory.DelegateBuildStrategy.Emit, info);
		    BenchStrategy(sw, trials, HandlerFactory.DelegateBuildStrategy.Expression, info);
			BenchStrategy(sw, trials, HandlerFactory.DelegateBuildStrategy.MethodInfo, info);
	    }

	    private void BenchStrategy(Stopwatch sw, int trials, HandlerFactory.DelegateBuildStrategy strategy, HandlerInfo info)
	    {
		    var noArgs = new object[] { };
			var d = _fixture.Factory.BuildHandler(info, strategy);
			sw.Restart();
		    for (var i = 0; i < trials; i++)
			    Assert.Equal("Hello, World!", d.DynamicInvoke(null, noArgs));
		    _console.WriteLine($"{strategy} {trials}x took {sw.Elapsed}");
	    }

	    [Fact]
	    public void Can_roundtrip_assembly()
	    {
		    var info = CreateDefaultHandler();
		    var a = _fixture.Factory.BuildAssemblyInMemory(info);
			Assert.NotNull(a);

		    var entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "hq"}.Main";
		    var t = a?.GetType(entrypoint);
		    var function = info.Function ?? "Execute";
		    var h = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);

			var r = (string)h?.Invoke(null, new object[] { });
		    Assert.Equal("Hello, World!", r);
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
