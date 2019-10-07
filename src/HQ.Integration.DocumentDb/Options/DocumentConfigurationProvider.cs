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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HQ.Common;
using HQ.Extensions.Options;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Configuration;

namespace HQ.Integration.DocumentDb.Options
{
	public class DocumentDbConfigurationProvider : ConfigurationProvider, ISaveConfigurationProvider
	{
		private readonly IDictionary<string, string> _ids;
		private readonly IDocumentDbRepository<ConfigurationDocument> _repository;
		private readonly DocumentConfigurationSource _source;

		public DocumentDbConfigurationProvider(DocumentConfigurationSource source)
		{
			_ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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

			foreach (var entry in Data)
			{
				var k = entry.Key;
				var v = entry.Value;

				if (v == null)
					continue;

				if (map.ContainsKey(k))
					continue;

				var id = _ids[k];
				if (_repository.DeleteAsync(id).GetAwaiter().GetResult())
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

				var document = _repository.UpsertAsync(new ConfigurationDocument {Key = k, Value = v}).GetAwaiter()
					.GetResult();
				_ids[k] = document.Id;
				changed = true;
			}

			return changed;
		}

		public bool Delete(string key)
		{
			var deleted = false;
			var keys = Data.Keys.ToList();

			for (var i = 0; i < keys.Count; i++)
			{
				var k = keys[i];
				if (k == key || k.StartsWith($"{key}:"))
				{
					var id = _ids[k];
					if (!_repository.DeleteAsync(id).GetAwaiter().GetResult())
						continue;

					Data.Remove(key);
					deleted = true;
				}
			}

			return deleted;
		}

		public override void Set(string key, string value)
		{
			if (TryGet(key, out var previousValue) && value == previousValue)
				return;

			var document = _repository.UpsertAsync(new ConfigurationDocument {Key = key, Value = value}).GetAwaiter()
				.GetResult();

			Data[key] = value;
			_ids[key] = document.Id;

			if (_source.ReloadOnChange)
				ReloadAndLog();
		}

		public override void Load()
		{
			var onChange = Data.Count > 0;
			Data.Clear();
			_ids.Clear();
			var data = _repository.RetrieveAsync().GetAwaiter().GetResult();
			var loadedKeys = 0;
			foreach (var item in data)
			{
				Data[item.Key] = item.Value;
				_ids[item.Key] = item.Id;
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