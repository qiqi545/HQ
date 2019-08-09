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
using System.Data;
using System.Data.Common;

namespace HQ.Integration.DocumentDb.Sql.DbProvider
{
	public sealed class DocumentDbDataAdapter : DbDataAdapter
	{
		protected override int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable,
			IDbCommand command, CommandBehavior behavior)
		{
			var rows = 0;
			var table = dataSet.Tables.Add();
			using (var reader = command.ExecuteReader())
				while (reader.Read())
				{
					var fieldNames = new List<string>(reader.FieldCount);

					for (var i = 0; i < reader.FieldCount; i++)
					{
						string fieldName;
						fieldNames.Add(fieldName = reader.GetName(i));
						if (table.Columns.Contains(fieldName))
							continue;
						var column = table.Columns.Add();
						column.ColumnName = fieldName;
						column.AllowDBNull = true;
					}

					var row = table.NewRow();
					foreach (var field in fieldNames)
						row[field] = reader[field];
					table.Rows.Add(row);
					rows++;
				}

			return rows;
		}
	}
}