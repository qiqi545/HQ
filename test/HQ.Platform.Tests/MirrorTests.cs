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
using HQ.Data.Contracts.Serialization;
using HQ.Test.Sdk;

namespace HQ.Platform.Tests
{
    public class MirrorTests : UnitUnderTest
    {
        [Test]
        public void ReadTests_SameVersion()
        {
            var person = new V2.Person { FirstName = "Kawhi", LastName = "Leonard" };

            var ms = new MemoryStream();
            person.Serialize(ms);

            var span = ms.GetBuffer().AsSpan();
            var mirror = new V2.PersonMirror(span);
            Assert.Equal(person.FirstName, mirror.FirstName);
            Assert.Equal(person.LastName, mirror.LastName);
        }

        [Test]
        public void ReadTests_NextVersion()
        {
            var person = new V1.Person {Name = "Kawhi"};

            var buffer = new byte[person.BufferSize];
            var ms = new MemoryStream(buffer);
            person.Serialize(ms);
            Assert.Equal(ms.Length, buffer.Length);

            var span = buffer.AsSpan();
            var mirror = new V2.PersonMirror(span);
            Assert.Equal(person.Name, mirror.FirstName);
        }

        [Test]
        public void ReadTests_PreviousVersion()
        {
            var person = new V2.Person {FirstName = "Kawhi", LastName = "Leonard"};

            var buffer = new byte[person.BufferSize];
            var ms = new MemoryStream(buffer);
            person.Serialize(ms);
            Assert.Equal(ms.Length, buffer.Length);

            var span = buffer.AsSpan();
            var mirror = new V1.PersonMirror(span);
            Assert.Equal(person.FirstName, mirror.Name);
        }

        [Test]
        public void WriteTests_SameVersion_MultipleRows()
        {
            var person1 = new V1.Person { Name = "Kawhi" };
            var person2 = new V1.Person { Name = "Kyle" };

            var buffer = new byte[person1.BufferSize + person2.BufferSize];
            var ms = new MemoryStream(buffer);
            person1.Serialize(ms);
            person2.Serialize(ms);
            Assert.Equal(ms.Length, buffer.Length);

            var span = buffer.AsSpan();

            var row1 = new V1.PersonMirror(span);
            Assert.Equal(person1.Name, row1.Name);

            var row2 = new V1.PersonMirror(span.Slice(person1.BufferSize));
            Assert.Equal(person2.Name, row2.Name);
        }

        [Test]
        public void WriteTests_DifferentVersions_MultipleRows()
        {
            var person1 = new V1.Person { Name = "Kawhi" };
            var person2 = new V2.Person { FirstName = "Kyle", LastName = "Lowry"};

            var buffer = new byte[person1.BufferSize + person2.BufferSize];
            var ms = new MemoryStream(buffer);
            person1.Serialize(ms);
            person2.Serialize(ms);
            Assert.Equal(ms.Length, buffer.Length);

            var span = buffer.AsSpan();

            var row11 = new V1.PersonMirror(span);
            Assert.Equal(person1.Name, row11.Name);

            var row12 = new V2.PersonMirror(span);
            Assert.Equal(person1.Name, row12.FirstName);

            var row21 = new V1.PersonMirror(span.Slice(person1.BufferSize));
            Assert.Equal(person2.FirstName, row21.Name);

            var row22 = new V2.PersonMirror(span.Slice(person1.BufferSize));
            Assert.Equal(person2.FirstName, row22.FirstName);
            Assert.Equal(person2.LastName, row22.LastName);
        }
    }
}
