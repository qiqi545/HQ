// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Blowdart.UI.Internal;
using Blowdart.UI.Internal.UriTemplates;
using Blowdart.UI.Web.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Blowdart.UI.Web
{
    public class HtmlSystem : UiSystem
    {
        private string _dom;
        private string _scripts;

        public StringBuilder Dom;
        public StringBuilder Scripts;

        public string RenderDom => _dom ?? Dom?.ToString();
        public string RenderScripts => _scripts ?? Scripts?.ToString();
        public IDictionary<string, object> Request { get; private set; }

        public override void Begin(UiContext context = null)
        {
            _dom = null;
            _scripts = null;
            Dom = StringBuilderHelper.Get();
            Scripts = StringBuilderHelper.Get();
            Request = context;
        }

        public override void End()
        {
            _dom = RenderDom;
            _scripts = RenderScripts;
            StringBuilderHelper.Return(Dom);
            StringBuilderHelper.Return(Scripts);
            Request.Clear();
        }
        
        public virtual string ScriptsSection()
        {
            return "<!-- SCRIPTS -->";
        }

        public virtual string StylesSection()
        {
            return "<!-- STYLES -->";
        }

        public override void PopulateAction(UiSettings settings, UiAction action, IServiceProvider serviceProvider, string template, object target, MethodInfo callee = null, Ui ui = null)
        {
            var http = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var options = serviceProvider.GetRequiredService<IOptions<UiServerOptions>>();
            
            var uriTemplate = new UriTemplate(template, caseInsensitiveParameterNames: true);
            var request = http.HttpContext.Request;
            var requestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute);

            //
            // URI Resolution:
            var templateParameters = uriTemplate.GetParameters(requestUri);
            var parameters = templateParameters ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (ui != null)
            {
                //
                // Header Resolution (overwrites URI):
                foreach (var entry in ui.Context)
                {
                    if (!entry.Key.StartsWith("bd.h:"))
                        continue;
                    parameters[entry.Key.Replace("bd.h:", string.Empty)] = entry.Value;
                }

                //
                // Query Resolution (overwrites headers):
                foreach (var entry in ui.Context)
                {
                    if (!entry.Key.StartsWith("bd.q:"))
                        continue;
                    parameters[entry.Key.Replace("bd.q:", string.Empty)] = entry.Value;
                }

                //
                // Form Collection (overwrites queries):
                foreach (var entry in ui.Context)
                {
                    if (!entry.Key.StartsWith("bd.f:"))
                        continue;
                    parameters[entry.Key.Replace("bd.f:", string.Empty)] = entry.Value;
                }
            }
            else
            {
                //
                // Header Resolution (overwrites URI):
                if (request.Headers != null)
                {
                    foreach (var headerName in request.Headers.Keys)
                    {
                        if (request.Headers.TryGetValue(headerName, out var value))
                            parameters[headerName.Replace("-", string.Empty)] = value;
                    }
                }

                //
                // Query Resolution (overwrites headers):
                foreach (var entry in request.Query)
                    parameters[entry.Key] = entry.Value;

                //
                // Form Collection (overwrites queries):
                if (request.HasFormContentType && request.Form != null)
                {
                    foreach (var parameterName in request.Form.Keys)
                    {
                        if (request.Form.TryGetValue(parameterName, out var value))
                        {
                            parameters[parameterName] = value;
                        }
                    }
                }
            }

            //
            // Routing:
            action.MethodName = IsRootPath(requestUri, options) ? callee?.Name ?? settings.DefaultMethodName : requestUri.Segments.FirstOrDefault();
            
            //
            // Execution:
            var targetType = callee?.DeclaringType ?? target.GetType();
            var parameterValues = parameters.Values.ToArray();
            var executor = targetType.GetExecutor(action.MethodName, parameterValues);
            if (executor.SameMethodParameters(parameterValues))
            {
                action.Arguments = parameterValues;
                return;
            }

            var arguments = Pools.ArgumentsPool.Get();
            try
            {
                foreach (var parameter in executor.MethodParameters)
                {
                    if (parameters.TryGetValue(parameter.Name, out var argument))
                    {
                        if (argument is StringValues multiString)
                        {
                            if (parameter.ParameterType == typeof(string))
                            {
                                arguments.Add(string.Join(",", multiString));
                            }
                            else
                            {
                                arguments.Add(multiString);
                            }
                        }
                        else
                        {
                            arguments.Add(argument);
                        }

                        continue;
                    }

                    if (parameter.ParameterType == typeof(Ui))
                    {
                        arguments.Add(ui ?? Ui.CreateNew(serviceProvider));
                        continue;
                    }

                    if (NotResolvableByContainer(parameter))
                    {
                        arguments.Add(null);
                        continue;
                    }

                    arguments.Add(serviceProvider.GetService(parameter.ParameterType));
                }

                action.Arguments = arguments.ToArray();
            }
            finally
            {
                Pools.ArgumentsPool.Return(arguments);
            }
        }

        private static bool NotResolvableByContainer(ParameterInfo parameter)
        {
            return parameter.ParameterType == typeof(string) || 
                   parameter.ParameterType == typeof(StringValues) ||
                   parameter.ParameterType == typeof(StringValues?) ||
                   parameter.ParameterType == typeof(byte) ||
                   parameter.ParameterType == typeof(byte?) ||
                   parameter.ParameterType == typeof(bool) ||
                   parameter.ParameterType == typeof(bool?) ||
                   parameter.ParameterType == typeof(short) ||
                   parameter.ParameterType == typeof(short?) ||
                   parameter.ParameterType == typeof(int) ||
                   parameter.ParameterType == typeof(int?) ||
                   parameter.ParameterType == typeof(long) ||
                   parameter.ParameterType == typeof(long?) ||
                   parameter.ParameterType == typeof(float) ||
                   parameter.ParameterType == typeof(float?) ||
                   parameter.ParameterType == typeof(double) ||
                   parameter.ParameterType == typeof(double?) ||
                   parameter.ParameterType == typeof(decimal) ||
                   parameter.ParameterType == typeof(decimal?) ||
                   parameter.ParameterType == typeof(DateTime) ||
                   parameter.ParameterType == typeof(DateTime?) ||
                   parameter.ParameterType == typeof(DateTimeOffset) ||
                   parameter.ParameterType == typeof(DateTimeOffset?) ||
                   parameter.ParameterType == typeof(TimeSpan) ||
                   parameter.ParameterType == typeof(TimeSpan?) ||
                   parameter.ParameterType == typeof(Guid) ||
                   parameter.ParameterType == typeof(Guid?);
        }

        private static bool IsRootPath(Uri requestUri, IOptions<UiServerOptions> options)
        {
            return requestUri.Segments.Length == 1 && requestUri.Segments[0] == "/" || 
                   requestUri.AbsolutePath == options.Value.HubPath;
        }
    }
}