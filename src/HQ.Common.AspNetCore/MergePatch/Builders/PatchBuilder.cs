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
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if NETCOREAPP3_0
namespace HQ.Common.AspNetCore.MergePatch.Builders
{
	public class PatchBuilder<TModel> where TModel : class
	{
		public JsonMergePatchDocument<TModel> Build(TModel original, TModel patched)
		{
			return PatchBuilder.Build(original, patched);
		}

		public JsonMergePatchDocument<TModel> Build(string jsonObjectPatch)
		{
			return PatchBuilder.Build<TModel>(jsonObjectPatch);
		}

		public JsonMergePatchDocument<TModel> Build(object jsonObjectPatch)
		{
			return PatchBuilder.Build<TModel>(jsonObjectPatch);
		}

		public JsonMergePatchDocument<TModel> Build(JObject jsonObjectPatch)
		{
			return PatchBuilder.Build<TModel>(jsonObjectPatch);
		}
	}

	public static class PatchBuilder
	{
		private static readonly JsonSerializer defaultSerializer = JsonSerializer.CreateDefault();

		#region Static methods

		public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched,
			JsonMergePatchOptions options = null) where TModel : class
		{
			return Build<TModel>(DiffBuilder.Build(original, patched) ?? new JObject(), options);
		}

		public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObjectPatch,
			JsonMergePatchOptions options = null) where TModel : class
		{
			return Build<TModel>(JObject.Parse(jsonObjectPatch), options);
		}

		public static JsonMergePatchDocument<TModel> Build<TModel>(object jsonObjectPatch,
			JsonMergePatchOptions options = null) where TModel : class
		{
			return Build<TModel>(JObject.FromObject(jsonObjectPatch), options);
		}

		public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObjectPatch,
			JsonMergePatchOptions options = null) where TModel : class
		{
			return CreatePatchDocument<TModel>(jsonObjectPatch, defaultSerializer,
				options ?? new JsonMergePatchOptions());
		}

		#endregion

		#region PatchCreation methods

		private static JsonMergePatchDocument<TModel> CreatePatchDocument<TModel>(JObject patchObject,
			JsonSerializer jsonSerializer, JsonMergePatchOptions options) where TModel : class
		{
			return CreatePatchDocument(typeof(JsonMergePatchDocument<TModel>), typeof(TModel), patchObject,
				jsonSerializer, options) as JsonMergePatchDocument<TModel>;
		}

		private static void AddOperation(JsonMergePatchDocument jsonMergePatchDocument, string pathPrefix,
			JObject patchObject, JsonMergePatchOptions options)
		{
			foreach (var jProperty in patchObject)
			{
				var path = pathPrefix + jProperty.Key;
				if (jProperty.Value is JValue jValue)
				{
					if (options.EnableDelete && jValue.Value == null)
						jsonMergePatchDocument.AddOperation_Remove(path);
					else
						jsonMergePatchDocument.AddOperation_Replace(path, jValue.Value);
				}
				else if (jProperty.Value is JArray jArray)
					jsonMergePatchDocument.AddOperation_Replace(path, jArray);
				else if (jProperty.Value is JObject jObject)
				{
					jsonMergePatchDocument.AddOperation_Add(path);
					AddOperation(jsonMergePatchDocument, path + "/", jObject, options);
				}
			}
		}

		internal static JsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType,
			JObject patchObject, JsonSerializer jsonSerializer, JsonMergePatchOptions options)
		{
			var model = patchObject.ToObject(modelType, jsonSerializer);
			var jsonMergePatchDocument = (JsonMergePatchDocument) Activator.CreateInstance(jsonMergePatchType,
				BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {model}, null);
			AddOperation(jsonMergePatchDocument, "/", patchObject, options);
			return jsonMergePatchDocument;
		}

		#endregion
	}
}
#endif