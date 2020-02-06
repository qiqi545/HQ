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
using System.Data.Common;
using HQ.Data.Sql.Queries;
using Microsoft.Azure.Documents;

namespace HQ.Integration.DocumentDb.Sql.DbProvider
{
	public static class DbCommandExtensions
	{
		public static SqlQuerySpec ToQuerySpec(this DbCommand command)
		{
			return new SqlQuerySpec(command.CommandText, new SqlParameterCollection(YieldParameters(command)));
		}

		private static IEnumerable<SqlParameter> YieldParameters(DbCommand command)
		{
			foreach (DbParameter parameter in command.Parameters)
				yield return new SqlParameter($"@{parameter.ParameterName}", parameter.Value);
		}

		public static SqlQuerySpec ToQuerySpec(this Query query)
		{
			return new SqlQuerySpec(query.Sql, new SqlParameterCollection(YieldParameters(query.Parameters)));
		}

		private static IEnumerable<SqlParameter> YieldParameters(IReadOnlyDictionary<string, object> parameters)
		{
			foreach (var parameter in parameters)
				yield return new SqlParameter($"@{parameter.Key}", parameter.Value);
		}
	}
}