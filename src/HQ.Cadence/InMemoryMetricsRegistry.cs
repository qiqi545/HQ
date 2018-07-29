// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HQ.Cadence
{
    public class InMemoryMetricsRegistry : IMetricsRegistry
    {
	    readonly ConcurrentDictionary<string, IMetricsHost> _registry;

	    public InMemoryMetricsRegistry()
	    {
		    _registry = new ConcurrentDictionary<string, IMetricsHost>();
	    }

		public void Add(IMetricsHost host)
		{
			var key = Environment.MachineName + "." + Environment.CurrentManagedThreadId;
			_registry.AddOrUpdate(key,
				host, (n, r) => r);
		}

	    public IEnumerator<IMetricsHost> GetEnumerator()
	    {
		    return _registry.Values.GetEnumerator();
	    }

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
