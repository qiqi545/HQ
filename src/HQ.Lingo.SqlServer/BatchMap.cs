using System.Collections.Generic;
using System.Data;

namespace HQ.Lingo.SqlServer
{
    public class BatchMap
    {
        public DataTable DataReaderTable { get; set; }
        public List<string> DatabaseTableColumns { get; set; }
        public List<string> SchemaTableColumns { get; set; }
    }
}
