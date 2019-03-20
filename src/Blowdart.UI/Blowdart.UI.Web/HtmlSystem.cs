// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blowdart.UI.Internal;
using Blowdart.UI.Internal.UriTemplates;
using Blowdart.UI.Web.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Blowdart.UI.Web
{
    public class HtmlSystem : UiSystem
    {
        private static readonly ObjectPool<List<object>> ArgumentsPool = new DefaultObjectPool<List<object>>(new DefaultPooledObjectPolicy<List<object>>());

        private string _dom;
        private string _scripts;

        public StringBuilder Dom;
        public StringBuilder Scripts;

        public string RenderDom => _dom ?? Dom?.ToString();
        public string RenderScripts => _scripts ?? Scripts?.ToString();

        public override void Begin()
        {
            _dom = null;
            _scripts = null;
            Dom = StringBuilderHelper.Get();
            Scripts = StringBuilderHelper.Get();
        }

        public override void End()
        {
            _dom = RenderDom;
            _scripts = RenderScripts;

            StringBuilderHelper.Return(Dom);
            StringBuilderHelper.Return(Scripts);
        }
        
        public virtual string ScriptsSection()
        {
            return "<!-- SCRIPTS -->";
        }

        public virtual string StylesSection()
        {
            return "<!-- STYLES -->";
        }

        public override void PopulateAction(UiSettings settings, UiAction action, IServiceProvider serviceProvider, string template, object target)
        {
            var http = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var options = serviceProvider.GetRequiredService<IOptions<UiServerOptions>>();
            
            var uriTemplate = new UriTemplate(template);
            var request = http.HttpContext.Request;
            var requestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute);

            //
            // URI Resolution:
            var parameters = uriTemplate.GetParameters(requestUri) ?? new Dictionary<string, object>();

            //
            // Header Resolution (overwrites):
            foreach (var parameterName in parameters.Keys)
            {
                var headerName = ParameterToHeader(parameterName);
                if (request.Headers.TryGetValue(headerName, out var value))
                    parameters[parameterName] = value;
            }

            //
            // Routing:
            action.MethodName = IsRootPath(requestUri, options) ? settings.DefaultMethodName : requestUri.Segments.FirstOrDefault();

            //
            // Parameter Resolution (does not overwrite):
            var parameterValues = parameters.Values.ToArray();
            var executor = target.GetType().GetExecutor(action.MethodName, parameterValues);
            if (executor.SameMethodParameters(parameterValues))
            {
                action.Arguments = parameterValues;
                return;
            }

            var arguments = ArgumentsPool.Get();
            try
            {
                foreach (var parameter in executor.MethodParameters)
                {
                    if (parameters.TryGetValue(parameter.Name, out var argument))
                    {
                        arguments.Add(argument);
                        continue;
                    }

                    if (parameter.ParameterType.IsValueType || parameter.ParameterType == typeof(string))
                    {
                        //
                        // Header/QueryString Resolution:
                        var headerName = ParameterToHeader(parameter.Name);
                        if (request.Headers.TryGetValue(headerName, out var value) || request.Query.TryGetValue(parameter.Name, out value))
                            arguments.Add(value);
                        else
                            arguments.Add(null);
                    }
                    else
                    {
                        arguments.Add(serviceProvider.GetService(parameter.ParameterType));
                    }
                }

                action.Arguments = arguments.ToArray();
            }
            finally
            {
                arguments.Clear();
                ArgumentsPool.Return(arguments);
            }
        }

        private static bool IsRootPath(Uri requestUri, IOptions<UiServerOptions> options)
        {
            return (requestUri.Segments.Length == 1 && requestUri.Segments[0] == "/") || 
                   (requestUri.AbsolutePath == options.Value.HubPath);
        }

        private static string ParameterToHeader(string parameterName)
        {
            return StringBuilderHelper.BuildString(sb =>
            {
                foreach (var c in parameterName)
                {
                    if (char.IsUpper(c))
                        sb.Append('-');
                    sb.Append(char.ToLowerInvariant(c));
                }
            });
        }
    }
}