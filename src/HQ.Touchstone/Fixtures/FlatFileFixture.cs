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

namespace HQ.Touchstone.Fixtures
{
    public class FlatFileFixture : TemporaryFileFixture
    {
        public FlatFileFixture(int lineCount, bool persistent = false) : base(persistent)
        {
            var data = new Lorem();
            for (var i = 0; i < lineCount; i++)
            {
                var buffer = Encoding.UTF8.GetBytes(data.Sentence() + Environment.NewLine);
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
