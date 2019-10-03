using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

#if NETCOREAPP3_0
namespace HQ.Common.AspNetCore.MergePatch
{
	public class JsonMergePatchDocument<TModel> : JsonMergePatchDocument where TModel : class
	{
		private static string replaceOp = OperationType.Replace.ToString();
		private static string addOp = OperationType.Add.ToString();
		private static string removeOp = OperationType.Remove.ToString();


		private readonly Type _modelType = typeof(TModel);
		private readonly JsonPatchDocument<TModel> _jsonPatchDocument = new JsonPatchDocument<TModel>();

		public TModel Model { get; internal set; }

		public override IContractResolver ContractResolver
		{
			get => _jsonPatchDocument.ContractResolver;
			set => _jsonPatchDocument.ContractResolver = value;
		}

		public List<Operation<TModel>> Operations
		{
			get => _jsonPatchDocument.Operations;
		}

		internal JsonMergePatchDocument(TModel model)
		{
			this.Model = model;
		}

#region Build Patch
		internal override void AddOperation_Replace(string path, object value)
			=> _jsonPatchDocument.Operations.Add(new Operation<TModel>(replaceOp, path, null, value));

		internal override void AddOperation_Remove(string path)
			=> _jsonPatchDocument.Operations.Add(new Operation<TModel>(removeOp, path, null, null));

		internal override void AddOperation_Add(string path)
		{
			var propertyType = ReflectionHelper.GetPropertyTypeFromPath(_modelType, path, ContractResolver);
			_jsonPatchDocument.Operations.Add(new Operation<TModel>(addOp, path, null, ContractResolver.ResolveContract(propertyType).DefaultCreator()));
		}
#endregion


		bool clean = false;
		private void ClearAddOperation(object objectToApplyTo)
		{
			if (clean)
				throw new NotSupportedException("Cannot apply more than once");

			var addOperations = _jsonPatchDocument.Operations.Where(operation => operation.OperationType == OperationType.Add).ToArray();
			foreach (var addOperation in addOperations)
			{
				if (ReflectionHelper.Exist(objectToApplyTo, addOperation.path, ContractResolver))
				{
					_jsonPatchDocument.Operations.Remove(addOperation);
				}
			}
			clean = true;
		}

		public TModel ApplyTo(TModel objectToApplyTo)
		{
			this.ClearAddOperation(objectToApplyTo);
			_jsonPatchDocument.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}

		public TOtherModel ApplyTo<TOtherModel>(TOtherModel objectToApplyTo) where TOtherModel : class
		{
			this.ClearAddOperation(objectToApplyTo);

			var newP = new JsonPatchDocument<TOtherModel>(
				_jsonPatchDocument
					.Operations
					.Select(o => new Operation<TOtherModel>(o.op, o.path, o.from, o.value))
					.ToList(),
				ContractResolver);

			newP.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}
	}
}
#endif