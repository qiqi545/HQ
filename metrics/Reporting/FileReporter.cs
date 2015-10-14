using System.IO;
using System.Text;

namespace metrics.Reporting
{
    /// <summary>
    /// A file-based reporter that outputs formatted metrics to a single flat file
    /// </summary>
    public class FileReporter : ReporterBase
    {
        public FileReporter(string path, Metrics metrics) : base(new StreamWriter(new FileStream(path, FileMode.Append), Encoding.UTF8, 8024, true),metrics)
        {
            
        }

        public FileReporter(string path, Encoding encoding, Metrics metrics)
            : base(new StreamWriter(new FileStream(path, FileMode.Append), encoding, 8024, true), metrics)
        {
            
        }

        public FileReporter(string path, IReportFormatter formatter)
            : base(new StreamWriter(new FileStream(path, FileMode.Append), Encoding.UTF8, 8024, true), formatter)
        {

        }

        public FileReporter(string path, Encoding encoding, IReportFormatter formatter) : base(new StreamWriter(new FileStream(path, FileMode.Append), encoding, 8024, true), formatter)
        {

        }
    }
}
