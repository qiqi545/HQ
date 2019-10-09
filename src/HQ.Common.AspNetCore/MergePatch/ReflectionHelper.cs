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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using TypeKitchen;

namespace HQ.Common.AspNetCore.MergePatch
{
	internal static class ReflectionHelper
	{
		private static readonly char[] pathSplitter = {'/'};

		private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			return type.GetProperties().Single(property =>
				property.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
		}

		internal static Type GetPropertyTypeFromPath(Type type, string path, IContractResolver contractResolver)
		{
			var currentType = type;
			var currentMembers = AccessorMembers.Create(type, AccessorMemberTypes.Fields | AccessorMemberTypes.Properties, AccessorMemberScope.Public);

			foreach (var propertyName in path.Split(pathSplitter, StringSplitOptions.RemoveEmptyEntries))
			{
				var jsonContract = contractResolver.ResolveContract(currentType);
				if (jsonContract is JsonDictionaryContract jsonDictionaryContract)
				{
					currentType = jsonDictionaryContract.DictionaryValueType;
					continue;
				}

				if (currentMembers.TryGetValue(propertyName, out var member))
					currentType = member.Type;
			}

			return currentType;
		}

		private static bool Exists(object value, IEnumerable<string> paths, IContractResolver contractResolver)
		{
			if (value == null)
				return false;

			var currentPath = paths.FirstOrDefault();
			if (currentPath == null)
				return value != null;

			object currentValue;

			var jsonContract = contractResolver.ResolveContract(value.GetType());
			if (jsonContract is JsonDictionaryContract)
			{
				try
				{
					currentValue = value
						.GetType()
						.GetProperty("Item")
						.GetValue(value, new[] {currentPath});
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
			{
				currentValue = GetPropertyInfo(value.GetType(), currentPath).GetValue(value);
			}


			return Exists(currentValue, paths.Skip(1), contractResolver);
		}

		internal static bool Exists(object value, string path, IContractResolver contractResolver)
		{
			return Exists(value, path.Split(pathSplitter, StringSplitOptions.RemoveEmptyEntries), contractResolver);
		}
	}
}