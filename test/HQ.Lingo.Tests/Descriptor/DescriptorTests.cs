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

using HQ.Lingo.Descriptor.TableDescriptor;
using HQ.Lingo.Tests.Descriptor.Models;
using HQ.Touchstone.Xunit;
using Xunit;

namespace HQ.Lingo.Tests.Descriptor
{
    public class DescriptorTests
    {
        [Test]
        public void Simple_user_has_an_identity()
        {
            var descriptor = SimpleDescriptor.Create<SimpleUser>();

            Assert.NotNull(descriptor);
            AssertIdentityIsPresent(descriptor);

            Assert.Equal("Id", descriptor.Identity.Property.Name);
            Assert.Equal(typeof(int), descriptor.Identity.Property.Type);

            Assert.Equal(1, descriptor.All.Count);
            Assert.Equal(0, descriptor.Insertable.Count);
        }

        [Test]
        public void Simple_inherited_user_has_an_identity()
        {
            var descriptor = SimpleDescriptor.Create<SimpleUserInherited>();

            Assert.NotNull(descriptor);
            AssertIdentityIsPresent(descriptor);

            Assert.Equal("Id", descriptor.Identity.Property.Name);
            Assert.Equal(typeof(int), descriptor.Identity.Property.Type);

            Assert.Equal(1, descriptor.All.Count);
            Assert.Equal(0, descriptor.Insertable.Count);
        }

        [Test]
        public void Through_tables_are_given_two_assigned_keys_rather_than_an_identity()
        {
            var descriptor = SimpleDescriptor.Create<ThroughTable>();
            Assert.NotNull(descriptor);
            Assert.Null(descriptor.Identity);

            Assert.Equal(2, descriptor.All.Count);
            Assert.Equal(2, descriptor.Insertable.Count);
            Assert.Equal(0, descriptor.Computed.Count);
            Assert.Equal(2, descriptor.Keys.Count);
            Assert.Equal(2, descriptor.Assigned.Count);
        }

        [Test]
        public void Transients_are_ignored()
        {
            var descriptor = SimpleDescriptor.Create<HasTransient>();
            Assert.NotNull(descriptor);
            AssertIdentityIsPresent(descriptor);

            Assert.Equal(2, descriptor.All.Count);
            Assert.Equal(1, descriptor.Insertable.Count);
        }

        [Test]
        public void Getting_and_setting_works_against_an_accessor()
        {
            var user = new SimpleUser();
            var descriptor = SimpleDescriptor.Create<SimpleUser>();
            var accessor = descriptor[0].Property;
            accessor.Set(user, 5);
            var value = accessor.Get(user);
            Assert.Equal(5, value);
        }

        [Test]
        public void Identity_attribute_overrides_multiple_keys()
        {
            var descriptor = SimpleDescriptor.Create<MultipleKeysButIdentityForced>();
            Assert.NotNull(descriptor.Identity);
            Assert.Equal(2, descriptor.Keys.Count);
        }

        private static void AssertIdentityIsPresent(Lingo.Descriptor.TableDescriptor.Descriptor descriptor)
        {
            Assert.NotNull(descriptor.Identity);
            Assert.Equal(1, descriptor.Computed.Count);
            Assert.Equal(1, descriptor.Keys.Count);
        }
    }
}
