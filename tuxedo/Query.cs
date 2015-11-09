using System.Collections.Generic;

namespace tuxedo
{
    public class Query
    {
        public string Sql { get; }

        public IDictionary<string, object> Parameters { get; private set; }

        public Query(string sql) : this(sql, new Dictionary<string, object>()) { }

        public Query(string sql, IDictionary<string, object> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }
        
        public override string ToString()
        {
            return Sql;
        }
    }
}