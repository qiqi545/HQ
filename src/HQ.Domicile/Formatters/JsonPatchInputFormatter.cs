// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Domicile.Models;
using HQ.Domicile.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace HQ.Domicile.Formatters
{
	public class JsonPatchInputFormatter : Microsoft.AspNetCore.Mvc.Formatters.JsonPatchInputFormatter
	{
		private readonly IDictionary<ITextTransform, JsonContractResolver> _resolvers;
		private JsonSerializer _serializer;

		public JsonPatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			MvcOptions options,
			MvcJsonOptions jsonOptions) :
			base(logger, serializerSettings, charPool, objectPoolProvider, options, jsonOptions)
		{
			_resolvers = new Dictionary<ITextTransform, JsonContractResolver>();
		}

		public override bool CanRead(InputFormatterContext context)
		{
			EnsureSerializer(context);

			return base.CanRead(context);
		}

		private void EnsureSerializer(InputFormatterContext context)
		{
			if (context.HttpContext.Items[HqContextKeys.JsonMulticase] is ITextTransform transform)
			{
				if (!_resolvers.TryGetValue(transform, out var resolver))
				{
					resolver = new JsonContractResolver(transform, JsonProcessingDirection.Input);
					_resolvers.Add(transform, resolver);
				}

				_serializer.ContractResolver = resolver;
			}
		}

		protected override JsonSerializer CreateJsonSerializer()
		{
			return _serializer ?? (_serializer = JsonSerializer.Create(SerializerSettings));
		}

		public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
		{
			return base.ReadRequestBodyAsync(context);
		}

		public override Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
		{
			return base.ReadAsync(context);
		}

		public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
			Encoding encoding)
		{
			return base.ReadRequestBodyAsync(context, encoding);
		}

		protected override void ReleaseJsonSerializer(JsonSerializer serializer)
		{
			_serializer = null;
		}
	}
}