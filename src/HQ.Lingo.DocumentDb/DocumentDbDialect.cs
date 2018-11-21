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
using System.Linq;
using System.Text;
using HQ.Lingo.Descriptor;
using HQ.Lingo.Dialects;
using LiteGuard;

namespace HQ.Lingo.DocumentDb
{
    public class DocumentDbDialect : ISqlDialect
    {
        protected internal const string Discriminator = "DocumentType";
        public char? StartIdentifier => null;
        public char? EndIdentifier => null;
        public char? Separator => '.';
        public char? Parameter => '@';
        public char? Quote => '\'';
        public string SetSuffix => string.Empty;

        public bool SupportsSelectStar => true;

        public bool TryFetchInsertedKey(FetchInsertedKeyLocation location, out string sql)
        {
            throw new NotImplementedException();
        }

        public void Page(string sql, StringBuilder sb)
        {
            throw new NotImplementedException();
        }

        public string ResolveTableName(IDataDescriptor descriptor)
        {
            return descriptor.Table;
        }

        public string ResolveColumnName(IDataDescriptor descriptor, string columnName)
        {
            return ResolveTableName(descriptor) + "." + columnName;
        }

        public IEnumerable<string> ResolveKeyNames(IDataDescriptor descriptor)
        {
            return descriptor.Keys.Select(c => ResolveColumnName(descriptor, c.ColumnName));
        }

        public IEnumerable<string> ResolveColumnNames(IDataDescriptor descriptor, ColumnScope scope = ColumnScope.All)
        {
            switch (scope)
            {
                case ColumnScope.All:
                    return descriptor.All.Select(c => ResolveColumnName(descriptor, c.ColumnName));
                case ColumnScope.Inserted:
                    return descriptor.Inserted.Select(c => ResolveColumnName(descriptor, c.ColumnName));
                case ColumnScope.Updated:
                    return descriptor.Updated.Select(c => ResolveColumnName(descriptor, c.ColumnName));
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }


        public bool BeforeSelect(IDataDescriptor descriptor, StringBuilder sb) => true;
        public bool BeforeSelectColumns(IDataDescriptor descriptor, StringBuilder sb, IList<string> columns) => true;

        public bool BeforeInsert(IDataDescriptor descriptor, StringBuilder sb) => true;

        public bool BeforeInsertColumns(IDataDescriptor descriptor, StringBuilder sb, IList<string> columns)
        {
            columns.Remove(ResolveColumnName(descriptor, "id"));
            return true;
        }

        public bool BeforeUpdate(IDataDescriptor descriptor, StringBuilder sb) => true;

        public bool BeforeUpdateColumns(IDataDescriptor descriptor, StringBuilder sb, IList<string> columns)
        {
            columns.Remove(ResolveColumnName(descriptor, "id"));
            return true;
        }

        public bool BeforeDelete(IDataDescriptor descriptor, StringBuilder sb) => true;

        public bool BeforeWhere(IDataDescriptor descriptor, StringBuilder sb, IList<string> keys) => true;

        public bool BeforeWhere(IDataDescriptor descriptor, StringBuilder sb, IList<string> keys,
            IList<string> parameters)
        {
            Guard.AgainstNullArgument(nameof(keys), keys);
            Guard.AgainstNullArgument(nameof(parameters), parameters);
            if (parameters.Contains(Discriminator))
                return true;
            keys.Add(ResolveColumnName(descriptor, Discriminator));
            parameters.Add(Discriminator);
            return true;
        }

        public bool AfterWhere(IDataDescriptor descriptor, StringBuilder sb, IList<string> keys) => true;

        public bool AfterWhere(IDataDescriptor descriptor, StringBuilder sb, IList<string> keys,
            IList<string> parameters) => true;
    }
}
