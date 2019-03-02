// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
                ui.Begin();
                _layoutRoot.Root(ui);
                ui.End();

                var system = _layoutRoot.Services.GetRequiredService<HtmlSystem>();
                await Clients.Caller.SendAsync(MessageTypes.FirstTimeDraw, system.RenderDom, system.RenderScripts);
            }
        }

        [HubMethodName("e")]
        public async Task HandleEvent(string page, string id, string eventType)
        {
            HandleEvent(_layoutRoot, page, id, eventType);

            if (!(_layoutRoot.Services.GetRequiredService<UiSystem>() is HtmlSystem system))
                throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

            await Clients.Caller.SendAsync(MessageTypes.ReplaceAll, system.RenderDom, system.RenderScripts);
            await Clients.Caller.SendAsync(MessageTypes.Log, id, eventType);
        }

        public void HandleEvent(LayoutRoot layout, string page, string id, string eventType)
        {
            var ui = Ui.CreateNew(layout.Services);

            switch (eventType)
            {
                case "click":
                {
                    ui.Begin();
                    ui.Clicked.Add(id);
                    layout.Root(ui);
                    ui.End();
                    break;
                }
                default:
                    throw new NotSupportedException(eventType);
            }
        }
    }
}