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
using System.Dynamic;
using System.Reflection;
using DotLiquid;

namespace HQ.Platform.Api.Notifications.Extensions
{
	internal static class HashExtensions
	{
		private static readonly IDictionary<Type, PropertyInfo[]>
			CachedStatics = new Dictionary<Type, PropertyInfo[]>();

		public static Hash FromDynamic(dynamic source)
		{
			var result = new Hash();
			if (source == null) return result;
			if (source is ExpandoObject dictionary)
				return Hash.FromDictionary(dictionary);
			var type = (Type) source.GetType();
			PropertyInfo[] properties;
			if (CachedStatics.ContainsKey(type))
				properties = CachedStatics[type];
			else
			{
				properties = type.GetProperties();
				CachedStatics.Add(type, properties);
			}

			foreach (var property in properties) result[property.Name] = property.GetValue(source, null);
			return result;
		}
	}
}