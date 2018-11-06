using System;
using System.Linq;
using System.Reflection;

namespace HQ.Common.Extensions
{
	internal static class TypeExtensions
	{
		public static ConstructorInfo GetWidestConstructor(this Type type)
		{
			var allPublic = type.GetConstructors();
			var constructor = allPublic.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
			return constructor ?? type.GetConstructor(Type.EmptyTypes);
		}

		public static MethodInfo GetWidestMethod(this Type type, string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			var allPublic = type.GetMethods().Where(m => m.Name.Equals(name, comparison));
			var method = allPublic.OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
			return method ?? type.GetMethod(name);
		}

		public static string GetNonGenericName(this Type type)
		{
			if (type == null)
				return null;
			var name = type.Name;
			var index = name.IndexOf('`');
			return index == -1 ? name : name.Substring(0, index);
		}
	}
}
