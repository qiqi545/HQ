/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
*/

using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;
using HQ.Data.Streaming;
using HQ.Extensions.Metrics;


namespace HQ.Template
{
    public static class PersonLayoutParser
    {
        public static PersonLayoutEnumerable Parse(Stream stream, Encoding encoding, string separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return Enumerate(stream, encoding, encoding.GetSeparatorBuffer(separator), maxWorkingMemoryBytes, metrics, cancellationToken);
        }

        public static PersonLayoutEnumerable Parse(Stream stream, Encoding encoding, byte[] workingBuffer, string separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            return Enumerate(stream, encoding, workingBuffer, encoding.GetSeparatorBuffer(separator), maxWorkingMemoryBytes, metrics, cancellationToken);
        }

        private static PersonLayoutEnumerable Enumerate(Stream stream, Encoding encoding, byte[] separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            var data = LineReader.StreamLines(stream, encoding, separator, maxWorkingMemoryBytes, metrics, cancellationToken);
            return new PersonLayoutEnumerable(data, encoding, separator);
        }

        private static PersonLayoutEnumerable Enumerate(Stream stream, Encoding encoding, byte[] workingBuffer, byte[] separator, int maxWorkingMemoryBytes = 0, IMetricsHost metrics = null, CancellationToken cancellationToken = default)
        {
            var data = LineReader.StreamLines(stream, encoding, workingBuffer, separator, maxWorkingMemoryBytes, metrics, cancellationToken);
            return new PersonLayoutEnumerable(data, encoding, separator);
        }
    }
}

