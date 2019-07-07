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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.UI.Web.Internal
{
	internal static class WebRenderer
	{
		private static string _template;

		public static async Task<bool> BuildUi(LayoutRoot layout, string template, HttpContext context)
		{
			var options = context.RequestServices.GetRequiredService<IOptions<UiServerOptions>>();
			var settings = context.RequestServices.GetRequiredService<UiSettings>();

			var path = context.Request.Path;

			var ui = Ui.CreateNew(context.RequestServices.GetRequiredService<UiData>());
			InlineElements.SetUi(ui);

			if (path == "/")
			{
				await Response(context, ui, path, layout.Root, layout, template, options.Value, settings);
				return true;
			}

			foreach (var page in layout.Handlers)
			{
				var pathString = new PathString(page.Key);
				if (!pathString.StartsWithSegments(path))
					continue;

				await Response(context, ui, page.Key, page.Value, layout, template, options.Value, settings);
				return true;
			}

			return false;
		}

		private static async Task Response(HttpContext context, Ui renderTarget, string pageKey,
			Action<Ui> renderAction, LayoutRoot layout, string template, UiServerOptions options, UiSettings settings)
		{
			var html = RenderToTarget(renderTarget, pageKey, renderAction, layout, template, context, options,
				settings);

			await WriteResponseAsync(html, context);
		}

		private static string RenderToTarget(Ui renderTarget, string pageKey, Action<Ui> renderAction,
			LayoutRoot layout, string template, HttpContext context, UiServerOptions options, UiSettings settings)
		{
			string html;
			if (options.UsePrerendering)
			{
				if (!layout.Systems.TryGetValue(pageKey, out var system))
					system = context.RequestServices.GetRequiredService<UiSystem>();

				if (!(system is HtmlSystem htmlSystem))
					throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

				var uiContext = WebUiContext.Build(context);

				renderTarget.Begin(system, uiContext);
				renderAction(renderTarget);
				renderTarget.End();

				if (htmlSystem.RenderDocument != null)
					return htmlSystem.RenderDocument;

				var bodySlug = $"<div id=\"{options.BodyElementId}\">";
				var scriptOpen = $"<script type=\"text/javascript\" id=\"{options.ScriptElementId}\">";
				var scriptClose = "</script>";
				var scriptSlug = scriptOpen + scriptClose;

#if DEBUG
				if (!template.Contains(bodySlug) || !template.Contains(scriptSlug))
					throw new Exception();
#endif
				const string titleKey = "title";

				var hasMeta = layout.Meta.TryGetValue(pageKey, out var meta);
				var title = !hasMeta ? settings.DefaultPageTitle : meta[titleKey] ?? settings.DefaultPageTitle;

				string metaString;
				var metaBuilder = Pooling.StringBuilderPool.Get();
				try
				{
					metaBuilder.Append("<title>").Append(title).Append("</title>").AppendLine();
					if (hasMeta)
						foreach (string name in meta.Keys)
						{
							if (name.Equals(titleKey, StringComparison.OrdinalIgnoreCase))
								continue;
							metaBuilder.AppendLine($"<meta name=\"{name}\" content=\"{meta[name]}\" />");
						}

					metaString = metaBuilder.ToString();
				}
				finally
				{
					Pooling.StringBuilderPool.Return(metaBuilder);
				}

				var stylesSection = htmlSystem.StylesSection();
				if (htmlSystem.RenderStyles != null)
					stylesSection += $"{Environment.NewLine}<style type=\"text/css\">{htmlSystem.RenderStyles}</style>";

				var scriptsSection = htmlSystem.ScriptsSection();

				string wasmSection;
				switch (options.DeployTarget)
				{
					case DeployTarget.Server:
						wasmSection = "";
						break;
					case DeployTarget.Client:
					case DeployTarget.Static:
						wasmSection = @"<!-- WASM -->
<script src=""~/wasm/mono.js""></script>
<script src=""~/wasm/mono.wasm""></script>
<script src=""~/wasm/wasm.js""></script>
";
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				html = template
						.Replace(bodySlug, bodySlug + htmlSystem.RenderDom)
						.Replace(scriptSlug,
							$"{scriptOpen}document.addEventListener(\"DOMContentLoaded\", function (event) {{{htmlSystem.RenderScripts}}});{scriptClose}")
						.Replace("<!-- META -->", metaString)
						.Replace("<!-- STYLES -->", stylesSection)
						.Replace("<!-- SCRIPTS -->", scriptsSection)
						.Replace("<!-- WASM -->", wasmSection)
					;
			}
			else
				html = template;

			return html;
		}

		public static string GetPageTemplate(IServiceProvider resolver)
		{
			return _template ?? (_template = LoadTemplate(resolver));
		}

		private static string LoadTemplate(IServiceProvider resolver)
		{
			var options = resolver.GetRequiredService<IOptions<UiServerOptions>>();
			var fp = resolver.GetRequiredService<IFileProvider>();
			var fi = fp.GetFileInfo(options.Value.TemplatePath);
			string template;
			using (var fs = fi.CreateReadStream())
			using (var ms = new MemoryStream())
			{
				fs.CopyTo(ms, (int) fi.Length);
				template = Encoding.UTF8.GetString(ms.GetBuffer());
			}

			return template;
		}

		private static async Task WriteResponseAsync(string html, HttpContext context)
		{
			if (context.IsTerminal())
				return;
			var options = context.RequestServices.GetRequiredService<IOptions<UiServerOptions>>();
			context.Response.ContentType = options.Value.ContentType;
			await context.Response.WriteAsync(html, Encoding.UTF8, context.RequestAborted);
		}

		private static bool IsTerminal(this HttpContext context)
		{
			return context.Response.HasStarted ||
			       context.Response.StatusCode != (int) HttpStatusCode.OK ||
			       context.Response.ContentType != null ||
			       context.Response.ContentLength.HasValue && context.Response.ContentLength > 0;
		}
	}
}