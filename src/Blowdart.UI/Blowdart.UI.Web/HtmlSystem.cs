// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blowdart.UI.Internal;
using Blowdart.UI.Internal.UriTemplates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace Blowdart.UI.Web
{
    public class HtmlSystem : UiSystem
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());
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
            Dom = StringBuilderPool.Get();
            Scripts = StringBuilderPool.Get();
        }

        public override void End()
        {
            _dom = RenderDom;
            _scripts = RenderScripts;

            StringBuilderPool.Return(Dom);
            StringBuilderPool.Return(Scripts);
        }

        internal string BuildString(Action<StringBuilder> action)
        {
            var sb = StringBuilderPool.Get();
            try
            {
                action(sb);
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        public override bool Button(Ui ui, string text)
        {
            throw new NotSupportedException("You must use a higher-order UI system, or raw DOM elements.");
        }

        public virtual string ScriptsSection()
        {
            return "<!-- SCRIPTS -->";
        }

        public virtual string StylesSection()
        {
            return "<!-- STYLES -->";
        }

        public override void PopulateAction(UiAction action, IServiceProvider serviceProvider, string template, object target)
        {
            var http = serviceProvider.GetRequiredService<IHttpContextAccessor>();

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
            string methodName;
            if (requestUri.Segments.Length == 1 && requestUri.Segments[0] == "/")
                methodName = nameof(LayoutRoot.Default);
            else
                methodName = requestUri.Segments.FirstOrDefault();

            action.MethodName = methodName;

            //
            // Parameter Resolution (does not overwrite):
            var parameterValues = parameters.Values.ToArray();
            var executor = target.GetType().GetExecutor(methodName, parameterValues);
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

        private static string ParameterToHeader(string parameterName)
        {
            var sb = StringBuilderPool.Get();
            try
            {
                foreach (var c in parameterName)
                {
                    if (char.IsUpper(c))
                        sb.Append('-');
                    sb.Append(char.ToLowerInvariant(c));
                }
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }
    }
}