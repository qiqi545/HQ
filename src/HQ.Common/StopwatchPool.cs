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
using Microsoft.Extensions.ObjectPool;

namespace HQ.Common
{
    public static class StopwatchPool
    {
        public static ObjectPool<Stopwatch> Pool = new LeakTrackingObjectPool<Stopwatch>(
            new DefaultObjectPool<Stopwatch>(new StopwatchPoolPolicy()));

        public static TimeSpan Scoped(Action<Stopwatch> closure)
        {
            var sw = Pool.Get();
            closure(sw);
            var elapsed = sw.Elapsed;
            Pool.Return(sw);
            return elapsed;
        }

        private class StopwatchPoolPolicy : IPooledObjectPolicy<Stopwatch>
        {
            public Stopwatch Create()
            {
                return Stopwatch.StartNew();
            }

            public bool Return(Stopwatch obj)
            {
                obj.Reset();
                return true;
            }
        }
    }
}
