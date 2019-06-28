using System;
using System.Data;
using Dapper;

namespace HQ.Data.Sql.Sqlite
{
    public class TimeSpanHandler : SqlMapper.TypeHandler<TimeSpan?>
    {
        protected TimeSpanHandler() { }

        public static readonly TimeSpanHandler Default = new TimeSpanHandler();

        public override void SetValue(IDbDataParameter parameter, TimeSpan? value)
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

        public override TimeSpan? Parse(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case TimeSpan timeSpan:
                    return timeSpan;
                default:
                    return TimeSpan.Parse(value.ToString());
            }
        }
    }
}