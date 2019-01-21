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
using System.IO;
using System.Text;
using Bogus.DataSets;
using HQ.Common.Helpers;

namespace HQ.Test.Sdk.Fixtures
{
    public class FlatFileFixture : TemporaryFileFixture
    {
        public FlatFileFixture(int lineCount, Encoding encoding, string separator = null, bool persistent = false) : base(persistent)
        {
            encoding = encoding ?? Encoding.UTF8;
            var data = new Lorem();
            for (var i = 0; i < lineCount; i++)
            {
                byte[] buffer;
                if (!string.IsNullOrWhiteSpace(separator))
                {
                    buffer = encoding.GetBytes(StringBuilderPool.Scoped(sb =>
                    {
                        var words = data.Words(data.Random.Number(1, 20));
                        for (var j = 0; j < words.Length; j++)
                        {
                            sb.Append(words[j]);
                            if (j < words.Length - 1)
                                sb.Append(separator);
                        }
                    }) + Environment.NewLine);
                }
                else
                {
                    buffer = encoding.GetBytes(data.Sentence() + Environment.NewLine);
                }
                FileStream.Write(buffer, 0, buffer.Length);
            }
            FileStream.Flush();
            if (persistent)
                FileStream.Close();
            else
                FileStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
