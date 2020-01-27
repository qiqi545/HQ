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
using System.Globalization;
using System.IO;
using System.Text;
using Bogus.DataSets;
using TypeKitchen;

namespace HQ.Data.Streaming.Benchmarks
{
    public class FlatFileFixture : TemporaryFileFixture
    {
        private static readonly Lorem Lorem = new Lorem(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

        public FlatFileFixture(int lineCount, int columnCount, Encoding encoding, string header = null, string separator = null, bool persistent = false) : this(
            lineCount, () => columnCount, encoding, header, separator, persistent) { }

        public FlatFileFixture(int lineCount, Encoding encoding, string header = null, string separator = null, bool persistent = false)
            : this(lineCount, Lorem.Random.Number(1, 20), encoding, header, separator, persistent) { }

        public FlatFileFixture(int lineCount, Func<int> columnCount, Encoding encoding, string header = null, string separator = null, bool persistent = false) : base(persistent)
        {
            encoding ??= Encoding.UTF8;
            var separated = !string.IsNullOrWhiteSpace(separator);

            byte[] buffer;
            if (!string.IsNullOrWhiteSpace(header))
            {
	            buffer = encoding.GetBytes(header + Environment.NewLine);
	            FileStream.Write(buffer, 0, buffer.Length);
            }

            if (separated)
            {
	            for (var i = 0; i < lineCount; i++)
	            {
		            var line = Pooling.StringBuilderPool.Scoped(sb =>
		            {
			            var words = Lorem.Words(columnCount());
			            for (var j = 0; j < words.Length; j++)
			            {
				            sb.Append(words[j]);
				            if (j < words.Length - 1)
					            sb.Append(separator);
			            }
		            });
		            buffer = encoding.GetBytes(line + Environment.NewLine);
		            FileStream.Write(buffer, 0, buffer.Length);
	            }
            }
            else
            {
	            for (var i = 0; i < lineCount; i++)
	            {
		            buffer = encoding.GetBytes(Lorem.Sentence() + Environment.NewLine);
		            FileStream.Write(buffer, 0, buffer.Length);
	            }
            }

            FileStream.Flush();
            if (persistent)
                FileStream.Close();
            else
                FileStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
