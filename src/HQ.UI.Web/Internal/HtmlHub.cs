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
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.UI.Web.Internal
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

			var http = Context.GetHttpContext();

			if (!_options.Value.UsePrerendering)
			{
				var ui = Ui.CreateNew(http.RequestServices.GetRequiredService<UiData>());
				ui.Begin(_layoutRoot.Systems["/"], WebUiContext.Build(this));
				_layoutRoot.Root(ui);
				ui.End();

				var system = http.RequestServices.GetRequiredService<HtmlSystem>();
				await Clients.Caller.SendAsync(MessageTypes.FirstTimeRender, system.RenderDom, system.RenderScripts);
			}
		}

		[HubMethodName("e")]
		public async Task HandleEvent(string page, string id, string eventType, byte[] data, string value)
		{
			var http = Context.GetHttpContext();

			var ui = Ui.CreateNew(http.RequestServices.GetRequiredService<UiData>());
			InlineElements.SetUi(ui);

			const string pageKey = "/"; // TODO: need to do our own reverse-match on the template to find the handler

			if (!_layoutRoot.Systems.TryGetValue(pageKey, out var system))
				system = http.RequestServices.GetRequiredService<UiSystem>();

			if (!(system is HtmlSystem htmlSystem))
				throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

			var delta = DeserializeInputStateDelta(data);
			var context = WebUiContext.Build(this, delta);

			ui.Begin(system, context);
			UpdateInputState(context, id, eventType, value, ui);
			_layoutRoot.Root(ui);

			if (ui.Invalidated)
			{
				await Clients.Caller.SendAsync(MessageTypes.Reload);
				return;
			}

			ui.End();
			await Clients.Caller.SendAsync(MessageTypes.Replace, htmlSystem.RenderDom, htmlSystem.RenderScripts);
		}

		private static JsonPatchDocument DeserializeInputStateDelta(byte[] data)
		{
			Console.WriteLine($"side channel buffer size: {data?.Length ?? 0}");

			var patch = data?.Length > 0 ? new JsonPatchDocument() : null;
			if (patch == null)
				return null;

			using (var ms = new MemoryStream(data))
			using (var br = new BinaryReader(ms))
				while (ms.Position < data.Length)
				{
					var op = br.ReadByte();
					switch (op)
					{
						case 0: // add
						{
							var index = br.ReadByte();
							patch.Add($"/{index}",
								new InputState
								{
									id = br.ReadString(),
									name = br.ReadString(),
									type = ((InputType) br.ReadByte()).ToString().ToLowerInvariant(),
									value = br.ReadString()
								});
							break;
						}

						case 1: // replace
						{
							var index = br.ReadByte();
							switch (br.ReadByte())
							{
								case 0:
									var patchId = br.ReadString();
									patch.Replace($"/{index}/id", patchId);
									break;
								case 1:
									var patchName = br.ReadString();
									patch.Replace($"/{index}/name", patchName);
									break;
								case 2:
									var patchType = ((InputType) br.ReadByte()).ToString().ToLowerInvariant();
									patch.Replace($"/{index}/type", patchType);
									break;
								case 3:
									var patchValue = br.ReadString();
									patch.Replace($"/{index}/value", patchValue);
									break;
								default:
									throw new ArgumentException();
							}

							break;
						}

						case 2: // remove
						{
							var path = br.ReadString();
							patch.Remove(path);
							break;
						}

						default:
							throw new ArgumentException();
					}
				}

			return patch;
		}

		private static void UpdateInputState(WebUiContext context, string id, string eventType, string value, Ui ui)
		{
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
			// DOM Events:
			switch (eventType)
			{
				case MouseEvents.mouseover:
				case MouseEvents.mouseout:
				case MouseEvents.click:
				{
					ui.Events.Add(eventType, id);
					break;
				}

				case InputEvents.input:
				{
					value = value.Trim('"');
					int.TryParse(value, out var val);
					ui.InputValues[id] = val;
					break;
				}

				default:
					throw new NotSupportedException(eventType);
			}
		}
	}
}