using System.Globalization;

namespace HQ.Evolve.Internal
{
    internal static class FloatExtensions
    {
        public static bool TryParseFast(string s, out float result)
        {
            // See: https://github.com/dotnet/coreclr/issues/20938
            var info = NumberFormatInfo.CurrentInfo;
            return float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, info, out result);
        }
    }
}
