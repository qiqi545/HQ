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

using System.Data;
using System.Linq;
using Dapper;

namespace HQ.Lingo.Dapper
{
    partial class TuxedoExtensions
    {
        public static T Insert<T>(this IDbConnection connection, T entity, IDbTransaction transaction = null,
            int? commandTimeout = null) where T : class
        {
            var descriptor = Tuxedo.GetDescriptor<T>();
            var insert = Tuxedo.Insert(entity);
            var sql = insert.Sql;
            if (descriptor.Identity != null)
            {
                sql = string.Concat(sql, "; ", Tuxedo.Identity());
                var result = connection.Query<int>(sql, Prepare(insert.Parameters), transaction, true, commandTimeout)
                    .Single();
                MapBackId(descriptor, entity, result);
            }
            else
            {
                connection.Execute(sql, Prepare(insert.Parameters), transaction, commandTimeout);
            }

            return entity;
        }
    }
}
