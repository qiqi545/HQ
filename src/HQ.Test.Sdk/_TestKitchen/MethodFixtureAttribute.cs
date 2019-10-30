using System;
using System.Reflection;
using Xunit.Sdk;

namespace TestKitchen
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class MethodFixtureAttribute : BeforeAfterTestAttribute
	{
		private object _instance;

		public Type Type { get; }

		public MethodFixtureAttribute(Type type)
		{
			Type = type;
		}

        public override void Before(MethodInfo methodUnderTest)
		{
			base.Before(methodUnderTest);

			_instance = Activator.CreateInstance(Type);
		}

		public override void After(MethodInfo methodUnderTest)
		{
			if(_instance is IDisposable disposable)
				disposable.Dispose();

			base.After(methodUnderTest);
		}
	}
}
