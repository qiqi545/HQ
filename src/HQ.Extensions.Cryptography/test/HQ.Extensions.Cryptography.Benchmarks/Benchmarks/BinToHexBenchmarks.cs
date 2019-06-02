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

using System;
using BenchmarkDotNet.Attributes;
using HQ.Extensions.Cryptography.Internal;
using Sodium;

namespace HQ.Extensions.Cryptography.Benchmarks.Benchmarks
{
    [CoreJob]
    [MarkdownExporter]
    [MemoryDiagnoser]
    public class BinToHexBenchmarks
    {
        private byte[] _buffer;
        [Params(512)] public int BufferSize;
        [Params(10000)] public int Trials;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _buffer = SodiumCore.GetRandomBytes(BufferSize);
        }

        [Benchmark(Baseline = true)]
        public void BinToHex_SystemNet()
        {
            for (var i = 0; i < Trials; i++)
            {
                Strings.BinToHex(_buffer, StringSource.SystemNet);
            }
        }

        [Benchmark]
        public void BinToHex_SodiumCore()
        {
            for (var i = 0; i < Trials; i++)
            {
                Strings.BinToHex(_buffer, StringSource.SodiumCore);
            }
        }

        [Benchmark]
        public void BinToHex_SodiumCoreDirect()
        {
            for (var i = 0; i < Trials; i++)
            {
                Strings.BinToHex(_buffer, StringSource.SodiumCoreDirect);
            }
        }

        [Benchmark]
        public void BinToHex_SodiumCoreUnsafePooled()
        {
            var span = new ReadOnlySpan<byte>(_buffer, 0, _buffer.Length);
            for (var i = 0; i < Trials; i++)
            {
                Strings.BinToHex(span, StringSource.SodiumCoreUnsafePooled);
            }
        }
    }
}
