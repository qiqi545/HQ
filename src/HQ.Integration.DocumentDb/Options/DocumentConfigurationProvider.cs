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
using HQ.Common;
using HQ.Extensions.Options;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Configuration;

namespace HQ.Integration.DocumentDb.Options
{
    public class DocumentConfigurationProvider : ConfigurationProvider, ISaveConfigurationProvider
    {
        private readonly DocumentConfigurationSource _source;
        private readonly IDocumentDbRepository<ConfigurationDocument> _repository;

        public DocumentConfigurationProvider(DocumentConfigurationSource source)
        {
            _source = source;
            _repository = new DocumentDbRepository<ConfigurationDocument>(Constants.Options.DefaultCollection, 
	            new OptionsMonitorShim<DocumentDbOptions>(source.Options), null);
		}

		public bool HasChildren(string key)
        {
            foreach (var entry in Data)
                if (entry.Key.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public bool Save<TOptions>(string key, TOptions instance)
        {
            var map = instance.Unbind(key);
            var changed = false;

            foreach (var (k, v) in Data)
            {
	            if (v == null)
		            continue;

	            if (map.ContainsKey(k))
		            continue;

	            if (_repository.DeleteAsync(k).GetAwaiter().GetResult())
		            changed = true;
            }

			foreach (var (k, v) in map)
            {
				Data.TryGetValue(k, out var value);

				var before = value;
				if (before == null && v == null)
		            continue; // no change

	            if (before != null && before.Equals(v, StringComparison.Ordinal))
		            continue; // no change

	            if (v == null)
		            continue; // not null constraint violation

	            var document = new ConfigurationDocument { Key = k, Value = v };
	            _repository.UpsertAsync(document).GetAwaiter().GetResult();
	            changed = true;
            }

			if (changed)
				Load();

			return changed;
        }

        public override void Set(string key, string value)
        {
            if (TryGet(key, out var previousValue) && value == previousValue)
                return;

			_repository.UpsertAsync(new ConfigurationDocument
			{
				Key = key,
				Value = value
			}).GetAwaiter().GetResult();

			Data[key] = value;
            if (_source.ReloadOnChange)
				ReloadAndLog();
		}

        public override void Load()
        {
            var onChange = Data.Count > 0;
            Data.Clear();
            var data = _repository.RetrieveAsync().GetAwaiter().GetResult();
            var loadedKeys = 0;
            foreach (var item in data)
            {
	            Data[item.Key] = item.Value;
	            onChange = true;
	            loadedKeys++;
            }

            if (loadedKeys > 0)
	            Trace.TraceInformation($"Configuration loaded {loadedKeys} keys from the store.");
			if (onChange && _source.ReloadOnChange)
				ReloadAndLog();
		}

        private void ReloadAndLog()
        {
	        OnReload();
	        Trace.TraceInformation("Configuration was reloaded.");
        }
    }
}
