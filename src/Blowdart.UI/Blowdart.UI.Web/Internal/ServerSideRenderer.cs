// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Blowdart.UI.Web.Internal
{
    internal static class ServerSideRenderer
    {
        public static async Task BuildUi(LayoutRoot layout, string template, HttpContext context, Func<Task> next)
        {
	        var serviceProvider = layout.Services;

	        var options = serviceProvider.GetRequiredService<IOptions<UiServerOptions>>();
            var settings = serviceProvider.GetRequiredService<UiSettings>();

            var ui = new ThreadLocal<Ui>(() =>
            {
	            var instance = Ui.CreateNew(serviceProvider);
				InlineElements.SetUi(instance);
				return instance;
            });

            var path = context.Request.Path;
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
            if (options.UseServerSideRendering)
            {
                renderTarget.Begin(WebUiContext.Build(context));
                renderAction(renderTarget);
                renderTarget.End();

                if (!(layout.Services.GetRequiredService<UiSystem>() is HtmlSystem system))
                    throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

                var bodySlug = $"<div id=\"{options.BodyElementId}\">";
                var scriptOpen = $"<script type=\"text/javascript\" id=\"{options.ScriptElementId}\">";
                var scriptClose = "</script>";
                var scriptSlug = scriptOpen + scriptClose;

#if DEBUG
                if (!template.Contains(bodySlug) || !template.Contains(scriptSlug))
                    throw new Exception();
#endif

	            string title;
	            if (!layout.Meta.TryGetValue(pageKey, out var meta))
		            title = settings.DefaultPageTitle;
	            else
		            title = meta["title"] ?? settings.DefaultPageTitle;

				html = template
                        .Replace(bodySlug, bodySlug + system.RenderDom)
                        .Replace(scriptSlug, $"{scriptOpen}function initUi() {{{system.RenderScripts}}};{scriptClose}")
                        .Replace("<!-- META -->", $"<title>{title}</title>")
						.Replace("<!-- STYLES -->", system.StylesSection())
                        .Replace("<!-- SCRIPTS -->", system.ScriptsSection())
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