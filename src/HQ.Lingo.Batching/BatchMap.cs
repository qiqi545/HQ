using System.Collections.Generic;
using System.Data;

namespace HQ.Lingo.Batching
{
    public class BatchMap
    {
        public DataTable DataReaderTable { get; set; }
        public IEnumerable<string> SchemaTableColumns { get; set; }
        public IEnumerable<string> DatabaseTableColumns { get; set; }
    }
}
