using System;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public class Audience
    {
        public static Lazy<Func<string, int, int>> Split = new Lazy<Func<string, int, int>>(OneBasedSplitOnHashCode);

        private static Func<string, int, int> OneBasedSplitOnHashCode()
        {
            return (identity, n) =>
            {
                var group = (int)(unchecked(((uint)identity.GetHashCode())) % n + 1);
                return group;
            };
        }
    }
}
