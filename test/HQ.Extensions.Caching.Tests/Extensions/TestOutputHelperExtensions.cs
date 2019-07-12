using Xunit.Abstractions;

namespace HQ.Extensions.Tests.Extensions
{
    internal static class TestOutputHelperExtensions
    {
        public static void WriteLine(this ITestOutputHelper helper, object value)
        {
            helper.WriteLine($"{value}");
        }
    }
}