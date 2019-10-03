using HQ.Common.AspNetCore.MergePatch.Builders;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace HQ.Common.AspNetCore.MergePatch
{
	public abstract class JsonMergePatchDocument
	{
		public const string ContentType = "application/merge-patch+json";

		internal abstract void AddOperation_Add(string path);
		internal abstract void AddOperation_Replace(string path, object value);
		internal abstract void AddOperation_Remove(string path);

		public abstract IContractResolver ContractResolver { get; set; }

		/// <summary>
		/// Returns Patch computed as "diff" of "patched - original"
		/// Warning: Only for tests - result.Model is not same as when user as InputFormatter
		/// </summary>
		public static JsonMergePatchDocument<TModel> Build<TModel>(TModel original, TModel patched) where TModel : class
			=> new PatchBuilder<TModel>().Build(original, patched);

		public static JsonMergePatchDocument<TModel> Build<TModel>(JObject jsonObject) where TModel : class
			=> new PatchBuilder<TModel>().Build(jsonObject);

		public static JsonMergePatchDocument<TModel> Build<TModel>(string jsonObject) where TModel : class
			=> new PatchBuilder<TModel>().Build(jsonObject);
	}
}