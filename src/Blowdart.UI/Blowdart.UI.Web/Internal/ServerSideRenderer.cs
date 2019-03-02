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
            var options = layout.Services.GetRequiredService<IOptions<UiServerOptions>>();
            var settings = layout.Services.GetRequiredService<UiSettings>();

            var ui = new ThreadLocal<Ui>(() => Ui.CreateNew(layout.Services));

            var path = context.Request.Path;
            if (path == "/")
            {
                await Response(ui.Value, layout.Root, layout, template, context, options.Value, settings);
            }
            else
            {
                foreach (var page in layout.Routes)
                {
                    var pathString = new PathString(page.Key);
                    if (!pathString.StartsWithSegments(path))
                        continue;
                    await Response(ui.Value, page.Value, layout, template, context, options.Value, settings);
                }

                await next();
            }
        }

        private static async Task Response(Ui renderTarget, Action<Ui> renderList, LayoutRoot layout, string template,
            HttpContext context, UiServerOptions options, UiSettings settings)
        {
            var html = RenderToTarget(renderTarget, renderList, layout, template, options, settings);

            await WriteResponseAsync(html, context);
        }

        private static string RenderToTarget(Ui renderTarget, Action<Ui> renderList, LayoutRoot layout, string template,
            UiServerOptions options, UiSettings settings)
        {
            string html;
            if (options.UseServerSideRendering)
            {
                renderTarget.Begin();
                renderList(renderTarget);
                renderTarget.End();

                if (!(layout.Services.GetRequiredService<UiSystem>() is HtmlSystem system))
                    throw new NotSupportedException(ErrorStrings.MustUseHtmlSystem);

                const string titleSlug = "<title></title>";
                const string domSlug = "<div id=\"ui-dom\">";
                const string scriptSlug = "<script type=\"text/javascript\" id=\"ui-scripts\">function initUi() { ";

                html = template
                        .Replace(titleSlug, $"<title>{settings.Title}</title>")
                        .Replace(domSlug, domSlug + system.RenderDom)
                        .Replace(scriptSlug, scriptSlug + system.RenderScripts)
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