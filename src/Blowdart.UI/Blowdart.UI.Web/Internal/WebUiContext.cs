// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Blowdart.UI.Web.Internal
{
    internal class WebUiContext : UiContext
    {
        public static UiContext Build(Hub hub)
        {
            return Build(hub.Context.GetHttpContext());
        }

        public static UiContext Build(HttpContext http)
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

            if(request.HasFormContentType && request.Form != null)
                foreach(var value in request.Form)
                    context.Add($"bd.f:{value.Key}", value.Value);

            return context;
        }
    }
}