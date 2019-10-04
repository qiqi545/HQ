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
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Serialization;

#if NETCOREAPP3_0
namespace HQ.Common.AspNetCore.MergePatch
{
	public class JsonMergePatchDocument<TModel> : JsonMergePatchDocument where TModel : class
	{
		private static readonly string replaceOp = OperationType.Replace.ToString();
		private static readonly string addOp = OperationType.Add.ToString();
		private static readonly string removeOp = OperationType.Remove.ToString();
		private readonly JsonPatchDocument<TModel> _jsonPatchDocument = new JsonPatchDocument<TModel>();


		private readonly Type _modelType = typeof(TModel);


		private bool clean;

		internal JsonMergePatchDocument(TModel model) => Model = model;

		public TModel Model { get; internal set; }

		public override IContractResolver ContractResolver
		{
			get => _jsonPatchDocument.ContractResolver;
			set => _jsonPatchDocument.ContractResolver = value;
		}

		public List<Operation<TModel>> Operations => _jsonPatchDocument.Operations;

		private void ClearAddOperation(object objectToApplyTo)
		{
			if (clean)
				throw new NotSupportedException("Cannot apply more than once");

			var addOperations = _jsonPatchDocument.Operations
				.Where(operation => operation.OperationType == OperationType.Add).ToArray();
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
			ClearAddOperation(objectToApplyTo);
			_jsonPatchDocument.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}

		public TOtherModel ApplyTo<TOtherModel>(TOtherModel objectToApplyTo) where TOtherModel : class
		{
			ClearAddOperation(objectToApplyTo);

			var newP = new JsonPatchDocument<TOtherModel>(
				_jsonPatchDocument
					.Operations
					.Select(o => new Operation<TOtherModel>(o.op, o.path, o.from, o.value))
					.ToList(),
				ContractResolver);

			newP.ApplyTo(objectToApplyTo);
			return objectToApplyTo;
		}

		#region Build Patch

		internal override void AddOperation_Replace(string path, object value)
		{
			_jsonPatchDocument.Operations.Add(new Operation<TModel>(replaceOp, path, null, value));
		}

		internal override void AddOperation_Remove(string path)
		{
			_jsonPatchDocument.Operations.Add(new Operation<TModel>(removeOp, path, null, null));
		}

		internal override void AddOperation_Add(string path)
		{
			var propertyType = ReflectionHelper.GetPropertyTypeFromPath(_modelType, path, ContractResolver);
			_jsonPatchDocument.Operations.Add(new Operation<TModel>(addOp, path, null,
				ContractResolver.ResolveContract(propertyType).DefaultCreator()));
		}

		#endregion
	}
}
#endif