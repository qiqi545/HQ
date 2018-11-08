// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HQ.Cadence
{
	public class InMemoryHealthChecksRegistry : IHealthChecksRegistry
	{
		private readonly ConcurrentDictionary<string, IHealthChecksHost> _registry;

		public InMemoryHealthChecksRegistry()
		{
			_registry = new ConcurrentDictionary<string, IHealthChecksHost>();
		}

		public void Add(IHealthChecksHost host)
		{
			var key = Environment.MachineName + "." + Environment.CurrentManagedThreadId;
			_registry.AddOrUpdate(key,
				host, (n, r) => r);
		}

		public IEnumerator<IHealthChecksHost> GetEnumerator()
		{
			return _registry.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}