using System.Collections.Generic;
using table_descriptor;

namespace tuxedo
{
    partial class Tuxedo
    {
        public static Query Insert<T>(T entity)
        {
            var descriptor = GetDescriptor<T>();
            var columnsToInsert = descriptor.Insertable;
            return Insert(entity, descriptor, columnsToInsert);
        }

        private static Query Insert<T>(T entity, IDescriptor descriptor, IList<PropertyToColumn> columnsToInsert)
        {
            var sql = InsertSql(descriptor, columnsToInsert);
            var parameters = ParametersFromInstance(entity, descriptor.Insertable);
            return new Query(sql, parameters);
        }

        private static string InsertSql(IDescriptor descriptor, IList<PropertyToColumn> columnsToInsert)
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