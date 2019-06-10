using System;

namespace HQ.Extensions.Correlation
{
    [Flags]
    public enum TraceFlags : byte
    {
        None = 0,
        Recorded = 1 << 0
    }
}