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

using System.Collections.Generic;
using HQ.Lingo.Descriptor.TableDescriptor;

namespace HQ.Lingo
{
    partial class Tuxedo
    {
        public static Query Insert<T>(T entity)
        {
            var descriptor = GetDescriptor<T>();
            var columnsToInsert = descriptor.Insertable;
            return Insert(entity, descriptor, columnsToInsert);
        }

        private static Query Insert<T>(T entity, Descriptor.TableDescriptor.Descriptor descriptor,
            IList<PropertyToColumn> columnsToInsert)
        {
            var sql = InsertSql(descriptor, columnsToInsert);
            var parameters = ParametersFromInstance(entity, descriptor.Insertable);
            return new Query(sql, parameters);
        }

        private static string InsertSql(Descriptor.TableDescriptor.Descriptor descriptor,
            IList<PropertyToColumn> columnsToInsert)
        {
            var sql = string.Concat(
                "INSERT INTO ", TableName(descriptor),
                " (", ColumnList(columnsToInsert), ") VALUES (",
                ColumnParameters(columnsToInsert).Concat(),
                ")");
            return sql;
        }
    }
}
