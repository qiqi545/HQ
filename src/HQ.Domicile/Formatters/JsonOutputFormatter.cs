// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Buffers;
using System.Collections.Generic;
using HQ.Common;
using HQ.Domicile.Models;
using HQ.Domicile.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace HQ.Domicile.Formatters
{
	public class JsonOutputFormatter : Microsoft.AspNetCore.Mvc.Formatters.JsonOutputFormatter
	{
		private readonly IDictionary<ITextTransform, JsonContractResolver> _resolvers;
		private JsonSerializer _serializer;

		public JsonOutputFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool) : base(
			serializerSettings, charPool)
		{
			_resolvers = new Dictionary<ITextTransform, JsonContractResolver>();
		}

		public override bool CanWriteResult(OutputFormatterCanWriteContext context)
		{
			EnsureSerializer(context);

			return base.CanWriteResult(context);
		}

		private void EnsureSerializer(OutputFormatterCanWriteContext context)
		{
			if (context.HttpContext.Items[HqContextKeys.JsonMulticase] is ITextTransform transform)
			{
				if (!_resolvers.TryGetValue(transform, out var resolver))
				{
					resolver = new JsonContractResolver(transform, JsonProcessingDirection.Output);
					_resolvers.Add(transform, resolver);
				}

				_serializer.ContractResolver = resolver;
			}
		}

		protected override JsonSerializer CreateJsonSerializer()
		{
			return _serializer ?? (_serializer = JsonSerializer.Create(SerializerSettings));
		}
	}
}