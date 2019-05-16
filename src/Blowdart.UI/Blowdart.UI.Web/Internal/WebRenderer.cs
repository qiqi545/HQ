// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blowdart.UI.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Blowdart.UI.Web.Internal
{
    internal static class WebRenderer
    {
        public static async Task BuildUi(LayoutRoot layout, string template, HttpContext context, Func<Task> next)
        {
	        var serviceProvider = layout.Services;

	        var options = serviceProvider.GetRequiredService<IOptions<UiServerOptions>>();
            var settings = serviceProvider.GetRequiredService<UiSettings>();

            var path = context.Request.Path;

			// todo this assumes that every request is in a dedicated thread, which is false
			var ui = new ThreadLocal<Ui>(() =>
            {
	            var instance = Ui.CreateNew(serviceProvider);
				InlineElements.SetUi(instance);
				return instance;
            });
			
            if (path == "/")
            {
                await Response(ui.Value, path, layout.Root, layout, template, context, options.Value, settings);
            }
            else
            {
                foreach (var page in layout.Handlers)
                {
                    var pathString = new PathString(page.Key);
                    if (!pathString.StartsWithSegments(path))
                        continue;
                    await Response(ui.Value, page.Key, page.Value, layout, template, context, options.Value, settings);
                }

                await next();
            }
        }

        private static async Task Response(Ui renderTarget, string pageKey, Action<Ui> renderAction, LayoutRoot layout, string template, HttpContext context, UiServerOptions options, UiSettings settings)
        {
            var html = RenderToTarget(renderTarget, pageKey, renderAction, layout, template, context, options, settings);

            await WriteResponseAsync(html, context);
        }

        private static string RenderToTarget(Ui renderTarget, string pageKey, Action<Ui> renderAction, LayoutRoot layout, string template, HttpContext context, UiServerOptions options, UiSettings settings)
        {
            string html;
            if (options.UsePrerendering)
            {
	            if (!layout.Systems.TryGetValue(pageKey, out var system))
		            system = layout.Services.GetRequiredService<UiSystem>();
	            if (!(system is HtmlSystem htmlSystem))
		            throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

				renderTarget.Begin(htmlSystem, WebUiContext.Build(context));
				renderAction(renderTarget);
                renderTarget.End();

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
				var metaBuilder = Pools.StringBuilderPool.Get();
				try
				{
					metaBuilder.Append("<title>").Append(title).Append("</title>").AppendLine();
					if (hasMeta)
					{
						foreach (string name in meta.Keys)
						{
							if (name.Equals(titleKey, StringComparison.OrdinalIgnoreCase))
								continue;
							metaBuilder.AppendLine($"<meta name=\"{name}\" content=\"{meta[name]}\" />");
						}
					}
					metaString = metaBuilder.ToString();
				}
				finally
				{
					Pools.StringBuilderPool.Return(metaBuilder);
				}

				var stylesSection = htmlSystem.StylesSection();
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
                        .Replace(scriptSlug, $"{scriptOpen}document.addEventListener(\"DOMContentLoaded\", function (event) {{{htmlSystem.RenderScripts}}});{scriptClose}")
                        .Replace("<!-- META -->", metaString)
						.Replace("<!-- STYLES -->", stylesSection)
                        .Replace("<!-- SCRIPTS -->", scriptsSection)
                        .Replace("<!-- WASM -->", wasmSection)
					;
            }
            else
            {
                html = template;
            }

            return html;
        }
        
        public static string LoadPageTemplate(IServiceProvider resolver, IOptions<UiServerOptions> options)
        {
            var fp = resolver.GetRequiredService<IFileProvider>();
            var fi = fp.GetFileInfo(options.Value.TemplatePath);
            string template;
            using (var fs = fi.CreateReadStream())
            {
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms, (int) fi.Length);
                    template = Encoding.UTF8.GetString(ms.GetBuffer());
                }
            }

            return template;
        }

        private static async Task WriteResponseAsync(string html, HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<IOptions<UiServerOptions>>();
            context.Response.StatusCode = (int) HttpStatusCode.OK;
            context.Response.ContentType = options.Value.ContentType;
            await context.Response.WriteAsync(html, Encoding.UTF8, context.RequestAborted);
        }
    }
}