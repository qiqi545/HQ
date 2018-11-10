using System;
using System.Diagnostics;
using Dynamitey;
using HQ.Touchstone;
using Xunit;

namespace Example.InteractionTests
{
    public class UnitTests : UnitUnderTest
    {
        [Test]
        public void Foo()
        {
            Trace.Write("This is a trace");

            LogCritical("This is a critical message");

            var dummy = Dummy<IMyInterface>();
            Assert.False(dummy.Method(5));

            var stub1 = Stub<IMyInterface>(new
            {
                Prop1 = "Test",
                Prop2 = 42L,
                Prop3 = Guid.Empty,
                Method = Method<bool>.Call<int>(it => it > 5)
            });
            Assert.True(stub1.Prop1 == "Test");
            Assert.True(stub1.Prop2 == 42L);
            Assert.True(stub1.Prop3 == Guid.Empty);
            Assert.True(stub1.Method(6));

            var stub2 = Stub<IMyInterface, MyImplementation>();
            Assert.True(stub2.Prop1 == "Test2");
            Assert.True(stub2.Prop2 == 43L);
            Assert.True(stub2.Prop3 == Guid.Empty);
            Assert.True(stub2.Method(6));

            var stub3 = Stub<IMyInterface, MyImplementation>(new
            {
                Method = Method<bool>.Call<int>(it => false)
            });
            Assert.True(stub3.Prop1 == "Test2");
            Assert.True(stub3.Prop2 == 43L);
            Assert.True(stub3.Prop3 == Guid.Empty);
            Assert.False(stub3.Method(6));

            var stub4 = Stub<IMyInterface, MyImplementation>(new MyImplementation { Prop2 = 44L });
            Assert.True(stub4.Prop2 == 44L);

            var stub5 = Stub<IMyInterface, MyImplementation>(new MyImplementation { Prop2 = 44L }, new { Prop2 = 45L });
            Assert.True(stub5.Prop2 == 45L);
        }

        public interface IMyInterface
        {
            string Prop1 { get; }
            long Prop2 { get; }
            Guid Prop3 { get; }
            bool Method(int x);
        }

        private class MyImplementation : IMyInterface
        {
            public string Prop1 { get; set; } = "Test2";
            public long Prop2 { get; set; } = 43L;
            public Guid Prop3 { get; set; } = Guid.Empty;
            public bool Method(int x) { return true; }
        }
    }
}
