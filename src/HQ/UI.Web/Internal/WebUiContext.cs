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
using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HQ.UI.Web.Internal
{
	internal class WebUiContext : UiContext
	{
		private static readonly Dictionary<string, List<InputState>> InputStateLookup =
			new Dictionary<string, List<InputState>>();

		private readonly Dictionary<string, StringValues> _formFields;
		private readonly Dictionary<string, StringValues> _queryStore;

		private WebUiContext()
		{
			Headers = new HeaderDictionary();

			_queryStore = new Dictionary<string, StringValues>();
			Query = new QueryCollection(_queryStore);

			_formFields = new Dictionary<string, StringValues>();
			Form = new FormCollection(_formFields);
		}

		public IFormCollection Form { get; }
		public IHeaderDictionary Headers { get; }
		public QueryCollection Query { get; }
		public ImmutableArray<InputState> InputState { get; set; }

		public override ClaimsPrincipal User
		{
			get
			{
				var http = UiServices.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
				return http?.HttpContext.User;
			}
		}

		public override void Clear()
		{
			base.Clear();

			Headers.Clear();
			_formFields.Clear();
			_queryStore.Clear();
			InputState = default;
		}

		public static WebUiContext Build(Hub hub, JsonPatchDocument stateDelta = null)
		{
			return Build(hub.Context.GetHttpContext(), stateDelta);
		}

		public static WebUiContext Build(HttpContext http, JsonPatchDocument stateDelta = null)
		{
			var request = http?.Request;
			if (request == null)
				return null;

			var context = new WebUiContext {UiServices = http.RequestServices, TraceIdentifier = http.TraceIdentifier};

			if (request.Headers != null)
				foreach (var value in request.Headers)
					context.Headers.Add(value.Key, value.Value);

			if (request.Query != null)
				foreach (var value in request.Query)
					context._queryStore.Add(value.Key, value.Value);

			// TODO using the form values directly is likely obsolete now and should be removed
			var hasFormData = request.HasFormContentType && request.Form != null;
			if (hasFormData)
			{
				if (stateDelta != null)
					throw new ArgumentException(
						"Library developer error: received the same data from multiple sources");

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