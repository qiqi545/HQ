// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.JsonPatch;
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

		public static WebUiContext Build(Hub hub, JsonPatchDocument stateDelta = null)
        {
            return Build(hub.Context.GetHttpContext(), stateDelta);
        }

        public static WebUiContext Build(HttpContext http, JsonPatchDocument stateDelta = null)
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
            
			// TODO using the form values directly is likely obsolete now and should be removed
			var hasFormData = request.HasFormContentType && request.Form != null;
            if (hasFormData)
            {
                if (stateDelta != null)
                    throw new ArgumentException("Library developer error: received the same data from multiple sources");

                foreach (var value in request.Form)
					context._formFields.Add(value.Key, value.Value);
			}
            else
            {
				var inputState = context.GetInputState();

				stateDelta?.ApplyTo(inputState);

				var gross = JArray.Parse(JsonConvert.SerializeObject(inputState)).Children();
				foreach (var input in gross)
				{
					var name = input["name"]?.Value<string>();
					if (string.IsNullOrWhiteSpace(name))
						continue;

					var v = input["value"]?.Value<string>();
					var value = string.IsNullOrWhiteSpace(v) ? null : v; // normalize on null
					context._formFields.Add(name, value);
				}

				context.InputState = inputState.ToImmutableArray();
            }
            
            return context;
        }

        internal List<InputState> GetInputState()
        {
	        var stateKey = GetInputStateKey();
	        if (!InputStateLookup.TryGetValue(stateKey, out var inputState))
		        InputStateLookup.Add(stateKey, inputState = new List<InputState>());
	        return inputState;
        }

        private string GetInputStateKey()
        {
			// TODO this is super wrong, needs dedicated middleware to harmonize multiple correlation IDs (i.e. anonymous, authenticated, converted, connection/client)
	        var affinity = User.Identity.Name ?? TraceIdentifier;
	        return affinity;
        }
    }
}