using HQ.Touchstone.Xunit;
using TableDescriptor.Tests.Models;
using Xunit;

namespace TableDescriptor.Tests
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
        
        private static void AssertIdentityIsPresent(Descriptor descriptor)
        {
            Assert.NotNull(descriptor.Identity);
            Assert.Equal(1, descriptor.Computed.Count);
            Assert.Equal(1, descriptor.Keys.Count);
        }
    }
}
