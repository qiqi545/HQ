using System;

namespace HQ.Common
{
	public static class ServiceProviderExtensions
	{
		public static string GetInnerGenericTypeName(this Type optionsWrapperType)
		{
			if (!optionsWrapperType.IsGenericParameter)
				return FallbackToGenericTypeName(optionsWrapperType);

			var declaringMethod = optionsWrapperType.DeclaringMethod;
			if (declaringMethod == null)
				return FallbackToGenericTypeName(optionsWrapperType);

			return declaringMethod.IsGenericMethod
				? declaringMethod.GetGenericArguments()[0].Name
				: declaringMethod.Name;
		}

		private static string FallbackToGenericTypeName(Type optionsWrapperType)
		{
			return optionsWrapperType.IsGenericType
				? optionsWrapperType.GetGenericArguments()[0].Name
				: optionsWrapperType.Name;
		}
	}
}
