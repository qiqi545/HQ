// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blowdart.UI.Web.Internal
{
    internal class WebUiContext : UiContext
    {
	    private readonly Dictionary<string, StringValues> _queryStore;
	    private readonly Dictionary<string, StringValues> _formFields;

	    public IFormCollection Form { get; }
	    public IHeaderDictionary Headers { get; }
		public QueryCollection Query { get; }
		public ImmutableArray<InputState> InputState { get; set; }

		private WebUiContext()
		{
			Headers = new HeaderDictionary();

			_queryStore = new Dictionary<string, StringValues>();
			Query = new QueryCollection(_queryStore);

			_formFields = new Dictionary<string, StringValues>();
			Form = new FormCollection(_formFields);
		}

		public override void Clear()
		{
			base.Clear();

			Headers.Clear();
			_formFields.Clear();
			_queryStore.Clear();
			InputState = default;
		}

		private static readonly Dictionary<string, List<InputState>> InputStateLookup = new Dictionary<string, List<InputState>>();

		public static WebUiContext Build(Hub hub, JsonPatchDocument data = null)
        {
            return Build(hub.Context.GetHttpContext(), data);
        }

        public static WebUiContext Build(HttpContext http, JsonPatchDocument data = null)
        {
            var request = http?.Request;
            if (request == null)
                return null;

			var context = new WebUiContext
            {
	            TraceIdentifier = http.TraceIdentifier,
				User = http.User
            };

			if (request.Headers != null)
            {
	            foreach (var value in request.Headers)
					context.Headers.Add(value.Key, value.Value);
			}

			if (request.Query != null)
			{
				foreach (var value in request.Query)
					context._queryStore.Add(value.Key, value.Value);
			}
            
			var hasFormData = request.HasFormContentType && request.Form != null;
            if (hasFormData)
            {
                if (data != null)
                    throw new ArgumentException("Library developer error: received the same data from multiple sources");

                foreach (var value in request.Form)
					context._formFields.Add(value.Key, value.Value);
			}
            else if(data != null)
            {
				var affinity = context.User.Identity.Name ?? context.TraceIdentifier;

				if (!InputStateLookup.TryGetValue(affinity, out var inputState))
					InputStateLookup.Add(affinity, inputState = new List<InputState>());

				data.ApplyTo(inputState);

				foreach (var input in JArray.Parse(JsonConvert.SerializeObject(inputState)).Children())
				{
					var name = input["name"]?.Value<string>();
					if (name == null)
						continue;

					var v = input["value"]?.Value<string>();
					var value = string.IsNullOrWhiteSpace(v) ? null : v; // normalize on null
					context._formFields.Add(name, value);
				}

				context.InputState = inputState.ToImmutableArray();
            }
            
            return context;
        }
    }
}