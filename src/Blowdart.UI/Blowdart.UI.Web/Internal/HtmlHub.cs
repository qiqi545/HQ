// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Blowdart.UI.Web.Internal
{
    internal class HtmlHub : Hub
    {
        private readonly LayoutRoot _layoutRoot;
        private readonly IOptions<UiServerOptions> _options;

        public HtmlHub(LayoutRoot layoutRoot, IOptions<UiServerOptions> options)
        {
            _layoutRoot = layoutRoot;
            _options = options;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            if (!_options.Value.UseServerSideRendering)
            {
                var ui = Ui.CreateNew(_layoutRoot.Services);
                ui.Begin(WebUiContext.Build(this));
                _layoutRoot.Root(ui);
                ui.End();

                var system = _layoutRoot.Services.GetRequiredService<HtmlSystem>();
                await Clients.Caller.SendAsync(MessageTypes.FirstTimeRender, system.RenderDom, system.RenderScripts);
            }
        }

        [HubMethodName("e")]
        public async Task HandleEvent(string page, string id, string eventType, string data)
        {
            HandleEvent(_layoutRoot, page, id, eventType, data);

            if (!(_layoutRoot.Services.GetRequiredService<UiSystem>() is HtmlSystem system))
                throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

            await Clients.Caller.SendAsync(MessageTypes.Replace, system.RenderDom, system.RenderScripts);
            await Clients.Caller.SendAsync(MessageTypes.Log, id, eventType);
        }

        public void HandleEvent(LayoutRoot layout, string page, string id, string eventType, string data)
        {
            var ui = Ui.CreateNew(layout.Services);
            var json = JArray.Parse(data);
            var context = WebUiContext.Build(this, json);

            ui.Begin(context);

            switch (eventType)
            {
                case "click":
                {
                    ui.Clicked.Add(id);
                    break;
                }
                default:
                    throw new NotSupportedException(eventType);
            }

            layout.Root(ui);
            ui.End();
        }
    }
}