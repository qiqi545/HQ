// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Flow.Tests.Fakes
{
	public class ThreadLocalScopedHandler : IConsumeScoped<BaseEvent>
	{
		private readonly ThreadLocal<List<string>> _cache;

		public ThreadLocalScopedHandler(ThreadLocal<List<string>> cache)
		{
			_cache = cache;
		}

		public List<string> Lines { get; } = new List<string>();

		public bool Before()
		{
			_cache.Value.Add("Before");
			return true;
		}

		public Task<bool> HandleAsync(BaseEvent message)
		{
			foreach (var line in _cache.Value)
				Lines.Add(line);
			return Task.FromResult(true);
		}

		public bool After(bool result)
		{
			if (result)
				_cache.Value.Add("After");
			return result;
		}
	}
}