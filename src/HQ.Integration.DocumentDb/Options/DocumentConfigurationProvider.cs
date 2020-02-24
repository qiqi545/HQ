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
using ActiveOptions;
using HQ.Common;
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

			lock (Data)
			{	
				//
				// Get all keys in the unbound instance that are *not* in our current view of configuration:
				var toDelete = new Dictionary<string, string>();
				foreach (var (k, v) in Data)
				{
					if (map.ContainsKey(k))
						continue; // we already know about this key

					toDelete.Add(k, _ids[k]);
					changed = true;
				}

				//
				// Get all keys in the current view of configuration that are *not* in the unbound instance:
				var toAdd = new Dictionary<string, string>();
				foreach (var (k, v) in map)
				{
					if (v == null)
						continue; // we don't have the key, but it's null, so we don't need it yet
					if (Data.ContainsKey(k))
						continue; // we already know about this key

					toAdd.Add(k, v);
					changed = true;
				}

				//
				// Get all keys in the unbound instance that are changing in the current view of configuration:
				var toUpdateIds = new Dictionary<string, string>();
				var toUpdateValues = new Dictionary<string, string>();
				
				foreach (var (k, after) in map)
				{
					if (!Data.TryGetValue(k, out var before))
						continue; // not an update

					if (before == null && after == null)
						continue; // no change

					if (before != null && before.Equals(after, StringComparison.Ordinal))
						continue; // no change

					if (after == null)
					{
						toDelete.Add(k, _ids[k]);
						changed = true;
						continue; // going from a value to null means we're deleting
					}

					toUpdateIds.Add(k, _ids[k]);
					toUpdateValues.Add(_ids[k], after);
					changed = true;
				}

				if (!changed)
					return false; // no changes

				foreach (var (k, v) in toAdd)
				{
					_repository.UpsertAsync(new ConfigurationDocument
					{
						Id = null,
						Key = k, 
						Value = v
					}).GetAwaiter().GetResult();
				}

				foreach (var (k, id) in toUpdateIds)
				{
					_repository.UpsertAsync(new ConfigurationDocument
					{
						Id = id,
						Key = k, 
						Value = toUpdateValues[id]
					}).GetAwaiter().GetResult();
				}
					
				foreach (var (k, v) in toDelete)
				{
					var id = v;
					var deleted = _repository.DeleteAsync(id).GetAwaiter().GetResult();
					if(deleted)
						_ids.Remove(k);
				}
			}

			return true;
		}

		public bool Delete(string key)
		{
			lock (Data)
			{
				var idsToDelete = new List<string>();
				var keysToDelete = new List<string>();

				foreach (var k in Data.Keys.Where(k => k == key || k.StartsWith($"{key}:")))
				{
					keysToDelete.Add(k);
					idsToDelete.Add(_ids[k]);
				}

				_repository.DeleteAsync(idsToDelete).GetAwaiter().GetResult();

				for (var i = 0; i < keysToDelete.Count; i++)
				{
					Data.Remove(keysToDelete[i]);
					_ids.Remove(idsToDelete[i]);
				}

				return true;
			}
		}

		public override void Set(string key, string value)
		{
			if (TryGet(key, out var previousValue) && value == previousValue)
				return;

			var document = _repository.UpsertAsync(new ConfigurationDocument {Key = key, Value = value}).GetAwaiter()
				.GetResult();

			lock(Data)
			{
				Data[key] = value;
				_ids[key] = document.Id;
			}

			if (_source.ReloadOnChange)
				ReloadAndLog();
		}

		public override void Load()
		{
			lock(Data)
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
				{
					lock (Trace.Listeners)
					{
						Trace.TraceInformation($"Configuration loaded {loadedKeys} keys from the store.");
					}
				}
				if (onChange && _source.ReloadOnChange)
					ReloadAndLog();
			}
		}

		private void ReloadAndLog()
		{
			OnReload();
			Trace.TraceInformation("Configuration was reloaded.");
		}
	}
}