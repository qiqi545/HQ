// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blowdart.UI.Web.Internal
{
	internal class InputState
	{
		public string id { get; set; }
		public string type { get; set; }
		public string value { get; set; }
	}

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
        public async Task HandleEvent(string page, string id, string eventType, string data, string value)
        {
			var ui = Ui.CreateNew(_layoutRoot.Services);
			InlineElements.SetUi(ui);

			var patch = JsonConvert.DeserializeObject<JsonPatchDocument>(data);
			var context = WebUiContext.Build(this, patch);

			const string pageKey = "/"; // TODO: need to do our own reverse-match on the template to find the handler

			if (!_layoutRoot.Systems.TryGetValue(pageKey, out var system))
				system = _layoutRoot.Services.GetRequiredService<UiSystem>();
			if (!(system is HtmlSystem htmlSystem))
				throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

			ui.Begin(system, context);
			Layout(id, eventType, context, value, ui);
			ui.End();

			Console.WriteLine($"HandleEvent: {page}|{id}|{eventType}");
			await Clients.Caller.SendAsync(MessageTypes.Replace, htmlSystem.RenderDom, htmlSystem.RenderScripts);
        }

        private void Layout(string id, string eventType, WebUiContext context, string value, Ui ui)
        {
			// TODO do this earlier on (when building UI?)
			//
	        // Input State:
	        foreach (var inputState in context.InputState)
	        {
		        if (id == inputState.id)
			        continue; // handled in the event state

		        switch (inputState.type)
		        {
			        case "range":
				        int.TryParse(inputState.value.Trim('"'), out var v);
				        ui.InputValues.Add(inputState.id, v);
				        break;
		        }
	        }

	        //
	        // Event State:
	        switch (eventType)
	        {
		        case MouseEvents.mouseover:
		        {
			        ui.MouseOver.Add(id);
			        break;
		        }
				case MouseEvents.mouseout:
		        {
			        ui.MouseOut.Add(id);
			        break;
		        }
				case MouseEvents.click:
		        {
			        ui.Clicked.Add(id);
			        break;
		        }
		        case InputEvents.input:
		        {
			        value = value.Trim('"');
			        int.TryParse(value, out var v);
			        ui.InputValues.Add(id, v);
			        break;
		        }
		        default:
			        throw new NotSupportedException(eventType);
	        }

	        _layoutRoot.Root(ui);
        }
    }
}