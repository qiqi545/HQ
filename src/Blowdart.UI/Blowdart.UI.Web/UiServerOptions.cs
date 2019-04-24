// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Blowdart.UI.Web
{
    public class UiServerOptions
    {
        public PathString HubPath { get; set; } = "/ui";
        public PathString LoggingPath { get; set; } = "/server/logs";
        public PathString TemplatePath { get; set; } = "/lib/index.html";
        public string ContentType { get; set; } = "text/html";
        public string BodyElementId { get; set; } = "ui-body";
        public string ScriptElementId { get; set; } = "ui-scripts";
        public bool UsePrerendering { get; set; } = true;
        public bool UseLogging { get; set; } = true;
        public ServerTransport MessagingModel { get; set; } = ServerTransport.All;
    }
}