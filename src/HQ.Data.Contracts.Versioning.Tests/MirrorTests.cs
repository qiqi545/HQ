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
using Xunit;

namespace HQ.Data.Contracts.Versioning.Tests
{
    public class MirrorTests
    {
        [Fact]
        public void ReadTests_Simple()
        {
            var person = new V2.Person { FirstName = "Kawhi", LastName = "Leonard"};
            
            var ms = new MemoryStream();
            person.Serialize(ms);

            var span = ms.GetBuffer().AsSpan();
            var mirror = new V2.PersonMirror(span);
            Assert.Equal(person.FirstName, mirror.FirstName);
            Assert.Equal(person.LastName, mirror.LastName);
        }

        [Fact]
        public void ReadTests_PreviousVersion()
        {
            var person = new V2.Person { FirstName = "Kawhi", LastName = "Leonard" };
            
            var buffer = new byte[1 + sizeof(int) + Encoding.UTF8.GetByteCount(person.FirstName) + 1 + sizeof(int) + Encoding.UTF8.GetByteCount(person.LastName)];
            var ms = new MemoryStream(buffer);
            person.Serialize(ms);
            Assert.Equal(ms.Length, buffer.Length);

            var span = buffer.AsSpan();
            var mirror = new V1.PersonMirror(span);
            Assert.Equal(person.FirstName, mirror.Name);
        }

        [Fact]
        public void ReadTests_NextVersion()
        {
            var person = new V1.Person { Name = "Kawhi" };
            
            var buffer = new byte[1 + sizeof(int) + Encoding.UTF8.GetByteCount(person.Name)];
            var ms = new MemoryStream(buffer);
            person.Serialize(ms);
            Assert.Equal(ms.Length, buffer.Length);

            var span = buffer.AsSpan();
            var mirror = new V2.PersonMirror(span);
            Assert.Equal(person.Name, mirror.FirstName);
        }
    }
}
