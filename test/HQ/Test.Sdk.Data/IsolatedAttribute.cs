using System;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace HQ.Test.Sdk.Data
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class IsolatedAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            base.Before(methodUnderTest);

            var fixture = typeof(ServiceUnderTest).GetProperty(nameof(ServiceUnderTest.AmbientServiceFixture))?.GetValue(null);
            if (fixture is IServiceFixture ambientServiceFixture && ambientServiceFixture.GetType().GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISupportIsolation)))
            {
                var instance = ambientServiceFixture;
                typeof(ISupportIsolation).GetMethod(nameof(ISupportIsolation.StartIsolation))?.Invoke(instance, new object[] { });
            }
        }

        public override void After(MethodInfo methodUnderTest)
        {
            var fixture = typeof(ServiceUnderTest).GetProperty(nameof(ServiceUnderTest.AmbientServiceFixture))?.GetValue(null);
            if (fixture is IServiceFixture ambientServiceFixture && ambientServiceFixture.GetType().GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISupportIsolation)))
            {
                var instance = ambientServiceFixture;
                typeof(ISupportIsolation).GetMethod(nameof(ISupportIsolation.EndIsolation))?.Invoke(instance, new object[] { });
            }

            base.After(methodUnderTest);
        }
    }
}
