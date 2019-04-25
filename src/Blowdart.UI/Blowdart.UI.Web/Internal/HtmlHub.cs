﻿// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Blowdart.UI.Internal.UriTemplates;
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

            if (!_options.Value.UsePrerendering)
            {
                var ui = Ui.CreateNew(_layoutRoot.Services);
                ui.Begin(_layoutRoot.Systems["/"], WebUiContext.Build(this));
                _layoutRoot.Root(ui);
                ui.End();

                var system = _layoutRoot.Services.GetRequiredService<HtmlSystem>();
                await Clients.Caller.SendAsync(MessageTypes.FirstTimeRender, system.RenderDom, system.RenderScripts);
            }
        }

        [HubMethodName("e")]
        public async Task HandleEvent(string page, string id, string eventType, string data)
        {
			var ui = Ui.CreateNew(_layoutRoot.Services);
			InlineElements.SetUi(ui);

			var json = JArray.Parse(data);
			var context = WebUiContext.Build(this, json);

			var pageKey = "/"; // TODO: need to do our own reverse-match on the template to find the handler

			if (!_layoutRoot.Systems.TryGetValue(pageKey, out var system))
				system = _layoutRoot.Services.GetRequiredService<UiSystem>();
			if (!(system is HtmlSystem htmlSystem))
				throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

			ui.Begin(system, context);

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

			_layoutRoot.Root(ui);
			ui.End();

			await Clients.Caller.SendAsync(MessageTypes.Replace, htmlSystem.RenderDom, htmlSystem.RenderScripts);
            await Clients.Caller.SendAsync(MessageTypes.Log, id, eventType);
        }
    }
}