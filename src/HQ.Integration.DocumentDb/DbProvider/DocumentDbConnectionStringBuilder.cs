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
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Azure.Documents.Client;

namespace HQ.Integration.DocumentDb.DbProvider
{
	public sealed class DocumentDbConnectionStringBuilder : DbConnectionStringBuilder
	{
		private readonly IDictionary<string, string> _settings;

		public DocumentDbConnectionStringBuilder()
		{
			_settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public DocumentDbConnectionStringBuilder(DocumentDbOptions options) : this()
		{
			Database = options.DatabaseId;
			DefaultCollection = options.CollectionId;
			SharedCollection = options.SharedCollection;
			AccountEndpoint = options.AccountEndpoint;
			AccountKey = options.AccountKey;
		}

		public void Bind(DocumentDbOptions options)
		{
			options.AccountEndpoint = AccountEndpoint;
			options.AccountKey = AccountKey;
			options.CollectionId = DefaultCollection;
			options.DatabaseId = Database;
			options.SharedCollection = SharedCollection;
			options.PartitionKeyPaths = PartitionKeyPaths;
		}
		
		public DocumentDbConnectionStringBuilder(string connectionString) : this()
		{
			var entries = connectionString.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
			var tokens = entries.Select(part => part.Split(new[] {'='}, 2));
			_settings = tokens.ToDictionary(split => split[0], split => split[1], StringComparer.OrdinalIgnoreCase);

			ConnectionString = ToString();
		}

		public Uri AccountEndpoint
		{
			get => this[Constants.AccountEndpointKey] is string endpoint ? new Uri(endpoint) : null;
			set
			{
				this[Constants.AccountEndpointKey] = value?.OriginalString;
				ConnectionString = ToString();
			}
		}

		public string AccountKey
		{
			get => this[Constants.AccountKeyKey] as string;
			set
			{
				this[Constants.AccountKeyKey] = value;
				ConnectionString = ToString();
			}
		}

		public string DefaultCollection
		{
			get => this[Constants.DefaultCollectionKey] as string;
			set
			{
				this[Constants.DefaultCollectionKey] = value;
				ConnectionString = ToString();
			}
		}

		public string[] PartitionKeyPaths
		{
			get => this[Constants.PartitionKeyPathsKey] as string[];
			set
			{
				this[Constants.PartitionKeyPathsKey] = value;
				ConnectionString = ToString();
			}
		}

		public string Database
		{
			get => this[Constants.DatabaseKey] as string;
			set
			{
				this[Constants.DatabaseKey] = value;
				ConnectionString = ToString();
			}
		}

		public bool SharedCollection
		{
			get => bool.TryParse(this[Constants.SharedCollectionKey] as string ?? "False", out var b) && b;
			set
			{
				this[Constants.SharedCollectionKey] = value;
				ConnectionString = ToString();
			}
		}

		public DocumentClient Build()
		{
			return new DocumentClient(AccountEndpoint, AccountKey, Defaults.JsonSettings);
		}

		#region DbConnectionStringBuilder

		public override int Count => _settings.Count;
		public override bool IsFixedSize => false;

		public override object this[string keyword]
		{
			get
			{
				_settings.TryGetValue(keyword, out var value);
				return value;
			}
			set => _settings[keyword] = value?.ToString();
		}

		public override ICollection Keys => (ICollection) _settings.Keys;
		public override ICollection Values => (ICollection) _settings.Values;

		public override bool ShouldSerialize(string keyword)
		{
			return _settings.ContainsKey(keyword);
		}

		public override void Clear()
		{
			_settings.Clear();
		}

		public override bool ContainsKey(string keyword)
		{
			return _settings.ContainsKey(keyword);
		}

		public override bool Remove(string keyword)
		{
			return _settings.Remove(keyword);
		}

		public override bool TryGetValue(string keyword, out object value)
		{
			if (_settings.TryGetValue(keyword, out var valueString))
			{
				value = valueString;
				return true;
			}

			value = default;
			return false;
		}

		public override bool EquivalentTo(DbConnectionStringBuilder connectionStringBuilder)
		{
			if (connectionStringBuilder is DocumentDbConnectionStringBuilder)
				return ConnectionString.Equals(connectionStringBuilder.ConnectionString,
					StringComparison.OrdinalIgnoreCase);

			throw new InvalidCastException($"The builder passed was not a {nameof(DbConnectionStringBuilder)}.");
		}

		protected override void GetProperties(Hashtable propertyDescriptors)
		{
			foreach (var entry in _settings)
				propertyDescriptors.Add(entry.Key, entry.Value);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var setting in _settings)
				sb.Append(setting.Key).Append("=").Append(setting.Value).Append(";");
			return sb.ToString();
		}

		#endregion
	}
}