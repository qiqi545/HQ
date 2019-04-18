// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.Operations;
using Newtonsoft.Json.Linq;

namespace Blowdart.UI.Web.Internal
{
    internal class WebUiContext : UiContext
    {
        public static UiContext Build(Hub hub, JArray data = null)
        {
            return Build(hub.Context.GetHttpContext(), data);
        }

        public static UiContext Build(HttpContext http, JArray data = null)
        {
            var request = http?.Request;
            if (request == null)
                return null;

            var context = new UiContext();

            if (request.Headers != null) 
                foreach(var value in request.Headers)
                    context.Add($"bd.h:{value.Key}", value.Value);

            foreach(var value in request.Query)
                context.Add($"bd.q:{value.Key}", value.Value);

            var hasFormData = request.HasFormContentType && request.Form != null;
            if (hasFormData)
            {
                if (data != null)
                    throw new ArgumentException("Library developer error: received the same data from multiple sources");

                foreach (var value in request.Form)
                    context.Add($"bd.f:{value.Key}", value.Value);
            }
            else if(data != null)
            {
                foreach (var element in data.Cast<JObject>())
                {
                    var name = element["name"];
                    var value = element["value"];

                    context.Add($"bd.f:{name}", value?.ToString());
                }
            }
            
            return context;
        }
    }
}