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
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQ.Common.AspNetCore.MergePatch.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HQ.Common.AspNetCore.MergePatch.Formatters
{
	internal class JsonMergePatchInputFormatter : NewtonsoftJsonInputFormatter
	{
		private static readonly MediaTypeHeaderValue JsonMergePatchMediaType =
			MediaTypeHeaderValue.Parse(JsonMergePatchDocument.ContentType).CopyAsReadOnly();

		private readonly IArrayPool<char> _charPool;
		private readonly JsonMergePatchOptions _mergePatchOptions;

		internal sealed class JsonArrayPool<T> : IArrayPool<T>
		{
			private readonly ArrayPool<T> _inner;

			public JsonArrayPool(ArrayPool<T> inner) =>
				_inner = inner ?? throw new ArgumentNullException(nameof(inner));

			public T[] Rent(int minimumLength)
			{
				return _inner.Rent(minimumLength);
			}

			public void Return(T[] array)
			{
				if (array == null)
					throw new ArgumentNullException(nameof(array));
				_inner.Return(array);
			}
		}

		public JsonMergePatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			MvcOptions mvcOptions,
			MvcNewtonsoftJsonOptions jsonOptions,
			JsonMergePatchOptions mergePatchOptions)
			: base(logger, serializerSettings, charPool, objectPoolProvider, mvcOptions, jsonOptions)
		{
			_charPool = new JsonArrayPool<char>(charPool);
			SupportedMediaTypes.Clear();
			SupportedMediaTypes.Add(JsonMergePatchMediaType);
			_mergePatchOptions = mergePatchOptions;
		}

		private static bool ContainerIsIEnumerable(InputFormatterContext context)
		{
			return context.ModelType.IsGenericType &&
			       context.ModelType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
		}

		private JsonMergePatchDocument CreatePatchDocument(Type jsonMergePatchType, Type modelType, JObject jObject,
			JsonSerializer jsonSerializer)
		{
			var jsonMergePatchDocument = PatchBuilder.CreatePatchDocument(jsonMergePatchType, modelType, jObject,
				jsonSerializer, _mergePatchOptions);
			jsonMergePatchDocument.ContractResolver = SerializerSettings.ContractResolver;
			return jsonMergePatchDocument;
		}

		public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
			Encoding encoding)
		{
			var request = context.HttpContext.Request;
			using (var streamReader = context.ReaderFactory(request.Body, encoding))
			{
				using (var jsonReader = new JsonTextReader(streamReader))
				{
					jsonReader.ArrayPool = _charPool;
					jsonReader.CloseInput = false;

					var jsonMergePatchType = context.ModelType;
					var container = (IList) null;

					if (ContainerIsIEnumerable(context))
					{
						jsonMergePatchType = context.ModelType.GenericTypeArguments[0];
						var listType = typeof(List<>);
						var constructedListType = listType.MakeGenericType(jsonMergePatchType);
						container = (IList) Activator.CreateInstance(constructedListType);
					}

					var modelType = jsonMergePatchType.GenericTypeArguments[0];


					var jsonSerializer = CreateJsonSerializer();
					try
					{
						var jToken = await JToken.LoadAsync(jsonReader);

						switch (jToken)
						{
							case JObject jObject:
								if (container != null)
									throw new ArgumentException(
										"Received object when array was expected"); //This could be handled by returnin list with single item

								var jsonMergePatchDocument = CreatePatchDocument(jsonMergePatchType, modelType, jObject,
									jsonSerializer);
								return await InputFormatterResult.SuccessAsync(jsonMergePatchDocument);
							case JArray jArray:
								if (container == null)
									throw new ArgumentException("Received array when object was expected");

								foreach (var jObject in jArray.OfType<JObject>())
								{
									container.Add(CreatePatchDocument(jsonMergePatchType, modelType, jObject,
										jsonSerializer));
								}

								return await InputFormatterResult.SuccessAsync(container);
						}

						return await InputFormatterResult.FailureAsync();
					}
					catch (Exception ex)
					{
						context.ModelState.TryAddModelError(context.ModelName, ex.Message);
						return await InputFormatterResult.FailureAsync();
					}
					finally
					{
						ReleaseJsonSerializer(jsonSerializer);
					}
				}
			}
		}


		public override bool CanRead(InputFormatterContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			var jsonMergePatchType = context.ModelType;

			if (ContainerIsIEnumerable(context))
				jsonMergePatchType = context.ModelType.GenericTypeArguments[0];

			return jsonMergePatchType.IsGenericType &&
			       jsonMergePatchType.GetGenericTypeDefinition() == typeof(JsonMergePatchDocument<>);
		}
	}
}