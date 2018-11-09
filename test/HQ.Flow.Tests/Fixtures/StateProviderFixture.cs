// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading;

namespace HQ.Flow.Tests.Fixtures
{
    public class StateProviderFixture : IDisposable
    {
        private static readonly object Sync = new object();

        public StateProviderFixture()
        {
            Monitor.Enter(Sync);
            StateProvider.Clear();
        }

        public void Dispose()
        {
            StateProvider.Clear();
            Monitor.Exit(Sync);
        }
    }
}
