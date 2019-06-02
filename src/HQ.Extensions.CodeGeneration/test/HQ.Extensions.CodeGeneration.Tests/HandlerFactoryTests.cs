#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Diagnostics;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Extensions.CodeGeneration.Tests
{
    public class HandlerFactoryTests : IClassFixture<HandlerFactoryFixture>
    {
        public HandlerFactoryTests(HandlerFactoryFixture fixture, ITestOutputHelper console)
        {
            _fixture = fixture;
            _console = console;
        }

        private readonly HandlerFactoryFixture _fixture;
        private readonly ITestOutputHelper _console;

        private void BenchStrategy(Stopwatch sw, int trials, DelegateBuildStrategy strategy, HandlerInfo info)
        {
            var noArgs = new object[] { };
            var d = _fixture.Factory.BuildCSharpHandler($"{Guid.NewGuid()}.dll", info, strategy);
            sw.Restart();
            for (var i = 0; i < trials; i++)
                Assert.Equal("Hello, World!", d.DynamicInvoke(null, noArgs));
            _console.WriteLine($"{strategy} {trials}x took {sw.Elapsed}");
        }

        private static HandlerInfo CreateDefaultHandler()
        {
            var info = new HandlerInfo
            {
                Namespace = "HelloWorld",
                Function = "Execute",
                Entrypoint = "HelloWorld.Main",
                Code = @"
namespace HelloWorld
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

        [Fact]
        public void Bench_all_strategies_vs_methodinfo()
        {
            var info = CreateDefaultHandler();

            const int trials = 1000000;
            var sw = Stopwatch.StartNew();

            var noArgs = new object[] { };
            var m = _fixture.Factory.BuildCSharpHandlerDirect($"{Guid.NewGuid()}.dll", info);
            sw.Restart();
            for (var i = 0; i < trials; i++)
                Assert.Equal("Hello, World!", m.Invoke(null, noArgs));
            _console.WriteLine($"MethodInfo (direct) {trials}x took {sw.Elapsed}");

            BenchStrategy(sw, trials, DelegateBuildStrategy.ObjectExecutor, info);
            BenchStrategy(sw, trials, DelegateBuildStrategy.Expression, info);
            BenchStrategy(sw, trials, DelegateBuildStrategy.MethodInfo, info);
        }

        [Fact]
        public void Can_build_default_handler()
        {
            var info = CreateDefaultHandler();
            var h = _fixture.Factory.BuildCSharpHandler($"{Guid.NewGuid()}.dll", info);
            var r = (string) h.DynamicInvoke(null, null);
            Assert.Equal("Hello, World!", r);
        }

        [Fact]
        public void Can_roundtrip_assembly()
        {
            var info = CreateDefaultHandler();
            var a = _fixture.Factory.BuildAssemblyInMemory($"{Guid.NewGuid()}.dll", info);
            Assert.NotNull(a);

            var entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "HelloWorld"}.Main";
            var t = a?.GetType(entrypoint);
            var function = info.Function ?? "Execute";
            var h = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);

            var r = (string) h?.Invoke(null, new object[] { });
            Assert.Equal("Hello, World!", r);
        }
    }
}
