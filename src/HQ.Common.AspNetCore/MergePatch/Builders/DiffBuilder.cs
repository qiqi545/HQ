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
using System.Linq;
using Newtonsoft.Json.Linq;

#if NETCOREAPP3_0
namespace HQ.Common.AspNetCore.MergePatch.Builders
{
	public static class DiffBuilder
	{
		public static JObject Build<TModel>(TModel original, TModel patched) where TModel : class
		{
			return Build(JObject.FromObject(original), JObject.FromObject(patched));
		}

		public static JObject Build(JObject original, JObject patched)
		{
			return (JObject) BuildDiff(original, patched);
		}

		private static JToken BuildDiff(JToken original, JToken patched)
		{
			var originalIsNull = original == null || original.Type == JTokenType.Null;
			var patchedIsNull = patched == null || patched.Type == JTokenType.Null;

			if (originalIsNull && patchedIsNull)
				return null;
			if (originalIsNull)
				return patched.DeepClone();
			if (patchedIsNull)
				return JValue.CreateNull();
			if (original is JArray || patched is JArray)
				return BuildArrayDiff(original as JArray, patched as JArray);
			switch (original)
			{
				case JValue originalValue:
					return BuildValueDiff(originalValue, patched as JValue);
				case JObject originalObject:
					return BuildObjectDiff(originalObject, patched as JObject);
				default:
					throw new NotImplementedException();
			}
		}

		private static JToken BuildObjectDiff(JObject original, JObject patched)
		{
			var result = new JObject();
			var properties = original?.Properties() ?? patched.Properties();
			foreach (var property in properties)
			{
				var propertyName = property.Name;
				var originalJToken = original?.GetValue(propertyName);
				var patchedJToken = patched?.GetValue(propertyName);

				var patchToken = BuildDiff(originalJToken, patchedJToken);
				if (patchToken != null)
					result.Add(propertyName, patchToken);
			}

			if (result.Properties().Any())
				return result;
			return null;
		}

		private static JValue BuildValueDiff(JValue original, JValue patched)
		{
			if (original.Value != null && !original.Value.Equals(patched.Value)
			    || patched.Value != null && !patched.Value.Equals(original?.Value))
				return patched.DeepClone() as JValue;

			return null;
		}

		private static JToken BuildArrayDiff(JArray original, JArray patched)
		{
			bool JArrayEquals(JArray left, JArray right)
			{
				if (left.Count != right.Count)
					return false;
				for (var i = 0; i < original.Count; i++)
				{
					//Hack.
					//Array can consist of values, objects or arrays so we reuse logic to calculate diff for each item
					//if there is any patch operation (aka any diff) we return false and replace whole array
					var diff = BuildDiff(left[i], right[i]);
					if (diff != null)
						return false;
				}

				return true;
			}

			if (JArrayEquals(original, patched))
				return null;
			return patched.DeepClone();
		}
	}
}
#endif