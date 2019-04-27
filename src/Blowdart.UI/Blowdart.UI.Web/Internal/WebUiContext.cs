// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
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
		}

		public static WebUiContext Build(Hub hub, JArray data = null)
        {
            return Build(hub.Context.GetHttpContext(), data);
        }

        public static WebUiContext Build(HttpContext http, JArray data = null)
        {
            var request = http?.Request;
            if (request == null)
                return null;

            var context = new WebUiContext
            {
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
                foreach (var element in data.Cast<JObject>())
                {
                    var name = element["name"]?.Value<string>();
                    if (name == null)
	                    continue;

                    var v = element["value"]?.Value<string>();
                    var value = string.IsNullOrWhiteSpace(v) ? null : v; // normalize on null
					context._formFields.Add(name, value);
                }
            }
            
            return context;
        }
    }
}