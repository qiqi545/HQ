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

using ActiveCaching;
using HQ.Platform.Api.Caching;

namespace HQ.Platform.Api.SplitTests
{
	public class CacheExperimentStore : IExperimentStore
	{
		private readonly CacheKeyValueStore<ExperimentKey, Experiment> _inner;

		public CacheExperimentStore(ICache cache, string keyGroup = null)
		{
			_inner = new CacheKeyValueStore<ExperimentKey, Experiment>(cache, keyGroup);
		}

		#region Implementation of IKeyValueStore<in ExperimentKey,Experiment>

		public Experiment GetOrAdd(ExperimentKey key, Experiment value)
		{
			return _inner.GetOrAdd(key, value);
		}

		public bool TryGetValue(ExperimentKey key, out Experiment value)
		{
			return _inner.TryGetValue(key, out value);
		}

		public bool Contains(ExperimentKey key)
		{
			return _inner.Contains(key);
		}

		public void AddOrUpdate<T>(ExperimentKey key, T value) where T : Experiment
		{
			_inner.AddOrUpdate(key, value);
		}

		public Experiment this[ExperimentKey key] => _inner[key];

		#endregion
	}
}