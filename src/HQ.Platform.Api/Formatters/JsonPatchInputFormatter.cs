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

using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace HQ.Platform.Api.Formatters
{
	public class JsonPatchInputFormatter : NewtonsoftJsonPatchInputFormatter
	{
		private readonly IDictionary<ITextTransform, JsonContractResolver> _resolvers;
		private JsonSerializer _serializer;

		public JsonPatchInputFormatter(
			ILogger logger,
			JsonSerializerSettings serializerSettings,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			MvcOptions options,
			MvcNewtonsoftJsonOptions jsonOptions
		) : base(logger, serializerSettings, charPool, objectPoolProvider, options, jsonOptions)
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
			if (context.HttpContext.Items[Constants.ContextKeys.JsonMultiCase] is ITextTransform transform)
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
			return _serializer ??= JsonSerializer.Create(SerializerSettings);
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