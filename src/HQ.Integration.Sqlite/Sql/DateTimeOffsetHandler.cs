using System;
using System.Data;
using Dapper;

namespace HQ.Integration.Sqlite.Sql
{
    public class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
    {
        protected DateTimeOffsetHandler() { }

        public static readonly DateTimeOffsetHandler Default = new DateTimeOffsetHandler();

        public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
        {
            if (value.HasValue)
            {
                parameter.Value = value.Value;
            }
            else
            {
                parameter.Value = DBNull.Value;
            }
        }

        public override DateTimeOffset? Parse(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case DateTimeOffset offset:
                    return offset;
                default:
                    return Convert.ToDateTime(value);
            }
        }
    }
}