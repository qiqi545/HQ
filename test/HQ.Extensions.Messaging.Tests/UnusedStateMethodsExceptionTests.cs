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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using HQ.Extensions.Messaging.Tests.Fixtures;
using HQ.Extensions.Messaging.Tests.States;
using Xunit;

namespace HQ.Extensions.Messaging.Tests
{
    public class UnusedStateMethodsExceptionTests
    {
        [Fact]
        public void Constructor_throws_when_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var exception = new UnusedStateMethodsException(null);
                exception.GetObjectData(null, new StreamingContext());
            });
        }

        [Fact]
        public void GetObjectData_throws_when_null()
        {
            using (new StateProviderFixture())
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    var exception = new UnusedStateMethodsException(typeof(MissingStateForStateMethod).GetMethods());
                    exception.GetObjectData(null, new StreamingContext());
                });
            }
        }

        [Fact]
        public void Round_trip_serialization_test()
        {
            var left = new UnusedStateMethodsException(typeof(MissingStateForStateMethod).GetMethods());
            var buffer = new byte[4096];

            var formatter = new BinaryFormatter();

            using (var serialized = new MemoryStream(buffer))
            {
                using (var deserialized = new MemoryStream(buffer))
                {
                    formatter.Serialize(serialized, left);

                    var right = (UnusedStateMethodsException) formatter.Deserialize(deserialized);

                    Assert.Equal(left.StateMethods, right.StateMethods);
                    Assert.Equal(left.InnerException?.Message, right.InnerException?.Message);
                    Assert.Equal(left.Message, right.Message);
                }
            }
        }
    }
}
