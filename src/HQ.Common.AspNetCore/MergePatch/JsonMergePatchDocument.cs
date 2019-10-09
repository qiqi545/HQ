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

using HQ.Common.AspNetCore.MergePatch.Builders;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace HQ.Common.AspNetCore.MergePatch
{
	public abstract class JsonMergePatchDocument
	{
		public const string ContentType = "application/merge-patch+json";

		public abstract IContractResolver ContractResolver { get; set; }

		internal abstract void AddOperation_Add(string path);
		internal abstract void AddOperation_Replace(string path, object value);
		internal abstract void AddOperation_Remove(string path);

		/// <summary>
		///     Returns Patch computed as "diff" of "patched - original"
		///     Warning: Only for tests - result.Model is not same as when user as InputFormatter
		/// </summary>
		public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched) where TModel : class
		{
			return new PatchBuilder<TModel>().Build(original, patched);
		}

		public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObject) where TModel : class
		{
			return new PatchBuilder<TModel>().Build(jsonObject);
		}

		public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObject) where TModel : class
		{
			return new PatchBuilder<TModel>().Build(jsonObject);
		}
	}
}