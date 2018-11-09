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
            if (context.HttpContext.Items[HqContextKeys.JsonMultiCase] is ITextTransform transform)
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
