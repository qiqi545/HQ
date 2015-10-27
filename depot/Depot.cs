using System;

namespace depot
{
    public static class Depot
    {
        static Depot()
        {
            ContentionTimeout = TimeSpan.FromSeconds(10);
        }

        public static TimeSpan ContentionTimeout { get; set; }
    }
}