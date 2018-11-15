// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.Azure.Documents.Client;

namespace System.Data.DocumentDb
{
    public sealed class DocumentDbConnectionStringBuilder : DbConnectionStringBuilder
    {
        private readonly IDictionary<string, string> _settings;

        public DocumentDbConnectionStringBuilder()
        {
            _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public DocumentDbConnectionStringBuilder(string connectionString) : this()
        {
            Guard.AgainstNullArgument(nameof(connectionString), connectionString);

            _settings = connectionString.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('='))
                .ToDictionary(split => split[0], split => split[1]);

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

        public string Database
        {
            get => this[Constants.DatabaseKey] as string;
            set
            {
                this[Constants.DatabaseKey] = value;
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
            get => _settings[keyword];
            set => _settings[keyword] = value?.ToString();
        }

        public override ICollection Keys => (ICollection) _settings.Keys;
        public override ICollection Values => (ICollection) _settings.Values;

        public override bool ShouldSerialize(string keyword)
        {
            Guard.AgainstNullArgument(nameof(keyword), keyword);
            return _settings.ContainsKey(keyword);
        }

        public override void Clear()
        {
            _settings.Clear();
        }

        public override bool ContainsKey(string keyword)
        {
            Guard.AgainstNullArgument(nameof(keyword), keyword);
            return _settings.ContainsKey(keyword);
        }

        public override bool Remove(string keyword)
        {
            Guard.AgainstNullArgument(nameof(keyword), keyword);
            return _settings.Remove(keyword);
        }

        public override bool TryGetValue(string keyword, out object value)
        {
            Guard.AgainstNullArgument(nameof(keyword), keyword);

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
            Guard.AgainstNullArgument(nameof(connectionStringBuilder), connectionStringBuilder);

            if (connectionStringBuilder is DocumentDbConnectionStringBuilder)
                return ConnectionString.Equals(connectionStringBuilder.ConnectionString,
                    StringComparison.OrdinalIgnoreCase);

            throw new InvalidCastException("The builder passed was not a " + nameof(DbConnectionStringBuilder) + ".");
        }

        protected override void GetProperties(Hashtable propertyDescriptors)
        {
            Guard.AgainstNullArgument(nameof(propertyDescriptors), propertyDescriptors);

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
