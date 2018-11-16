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

namespace HQ.Lingo.DocumentDb
{
    public class DocumentDbDialect : ISqlDialect
    {
        public char? StartIdentifier => null;
        public char? EndIdentifier => null;
        public char? Separator => '.';
        public char? Parameter => '@';
        public char? Quote => '\'';

        public string SetSuffix => string.Empty;
        public bool SelectStar => false;

        protected internal const string Discriminator = "DocumentType";

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

        public bool BeforeWhere(IDataDescriptor descriptor, StringBuilder sb, IList<string> keys, IList<string> parameters)
        {
            Guard.AgainstNullArgument(nameof(keys), keys);
            Guard.AgainstNullArgument(nameof(parameters), parameters);
            if (parameters.Contains(Discriminator))
                return true;
            keys.Add(ResolveColumnName(descriptor, Discriminator));
            parameters.Add(Discriminator);
            return true;
        }

        public bool AfterWhere(IDataDescriptor descriptor, StringBuilder sb, IList<string> keys, IList<string> parameters) => true;
        public bool BeforeSelect(IDataDescriptor descriptor, StringBuilder sb) => true;
        public bool BeforeSelectColumns(IDataDescriptor descriptor, StringBuilder sb, IList<string> columns) => true;
    }
}
