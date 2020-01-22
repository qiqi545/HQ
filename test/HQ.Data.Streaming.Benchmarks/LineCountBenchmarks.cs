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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using HQ.Test.Sdk.Fixtures;

namespace HQ.Data.Streaming.Benchmarks
{
	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporter]
    [MemoryDiagnoser]
    [CsvMeasurementsExporter]
    [RPlotExporter]
    public class LineCountBenchmarks
    {
        private Dictionary<int, string> _files;

        [Params(10_000_000)] public int RowCount;

        [GlobalSetup]
        public void Setup()
        {
            _files = new Dictionary<int, string>
            {
                {10_000_000, new FlatFileFixture(10_000_000, Encoding.UTF8, null, true).FilePath}
            };
        }

        [GlobalCleanup]
        public void CleanUp()
        {
            foreach (var file in _files.Values)
            {
                File.Delete(file);
            }
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = 1)]
        public int File_ReadLines()
        {
            return File.ReadLines(_files[RowCount], Encoding.UTF8).Count();
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public long LineReader_CountLines()
        {
            return LineReader.CountLines(File.OpenRead(_files[RowCount]), Encoding.UTF8, null, CancellationToken.None);
        }
    }
}
