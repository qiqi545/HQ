// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;

namespace System.Data.DocumentDb
{
    public sealed class DocumentDbDataAdapter : DbDataAdapter
    {
        protected override int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable,
            IDbCommand command, CommandBehavior behavior)
        {
            var rows = 0;
            var table = dataSet.Tables.Add();
            using (var reader = command.ExecuteReader())
            {
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
            }

            return rows;
        }
    }
}
