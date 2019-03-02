// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Blowdart.UI.Web
{
    public class UiServerOptions
    {
        public PathString HubPath { get; set; } = "/ui";
        public PathString TemplatePath { get; set; } = "/lib/index.html";
        public string ContentType { get; set; } = "text/html";
        public bool UseServerSideRendering { get; set; } = true;
        public ServerTransport MessagingModel { get; set; } = ServerTransport.All;
    }
}