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
	    private string _styles;
	    private string _dom;
	    private string _scripts;

	    public StringBuilder Styles;
		public StringBuilder Dom;
		public StringBuilder Scripts;

		public string RenderStyles => _styles ?? Styles?.ToString();
		public string RenderDom => _dom ?? Dom?.ToString();
        public string RenderScripts => _scripts ?? Scripts?.ToString();
        
		public override void Begin(UiContext context = null)
		{
			_styles = null;
            _dom = null;
            _scripts = null;

            Styles = Pools.StringBuilderPool.Get();
            Dom = Pools.StringBuilderPool.Get();
            Scripts = Pools.StringBuilderPool.Get();
        }

        public override void End()
        {
	        _styles = RenderStyles;
            _dom = RenderDom;
            _scripts = RenderScripts;

            Pools.StringBuilderPool.Return(Styles);
			Pools.StringBuilderPool.Return(Dom);
            Pools.StringBuilderPool.Return(Scripts);
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

			var request = http.HttpContext.Request;
			var requestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute);

			// if we don't use the HTTP service provider, we can't resolve any scoped services!
			serviceProvider = http.HttpContext.RequestServices;

			var uriTemplate = new UriTemplate(template, caseInsensitiveParameterNames: true);
            
            //
            // URI Resolution:
            var templateParameters = uriTemplate.GetParameters(requestUri);
            var parameters = templateParameters ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (ui?.Context is WebUiContext context)
            {
                //
                // Header Resolution (overwrites URI):
                foreach (var entry in context.Headers)
					parameters[entry.Key] = entry.Value;

				//
				// Query Resolution (overwrites headers):
				foreach (var entry in context.Query)
					parameters[entry.Key] = entry.Value;

				//
				// Form Collection (overwrites queries):
				foreach (var entry in context.Form)
					parameters[entry.Key] = entry.Value;
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
            action.MethodName = IsRootPath(requestUri, options) ? callee?.Name ?? settings.DefaultPageMethodName : requestUri.Segments.LastOrDefault();
            
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
                    if (parameters.TryGetValue(parameter.Name, out var parameterValue))
                    {
                        if (parameterValue is StringValues multiString)
                        {
                            if (parameter.ParameterType == typeof(string))
                            {
	                            arguments.Add(string.IsNullOrWhiteSpace(multiString) ? null : string.Join(",", multiString));
                            }
                            else
                            {
                                arguments.Add(multiString);
                            }
                        }
                        else
                        {
                            arguments.Add(parameterValue);
                        }

                        continue;
                    }

                    if (parameter.ParameterType == typeof(Ui))
                    {
	                    ui = ui ?? InlineElements.GetUi() ?? throw new ArgumentNullException(nameof(ui), 
		                         $"No UI instance passed to {nameof(PopulateAction)} or found in this synchronization context");

	                    arguments.Add(ui);
                        continue;
                    }

                    if (NotResolvableByContainer(parameter))
                    {
                        arguments.Add(null);
                        continue;
                    }

                    try
                    {
	                    var argument = serviceProvider.GetService(parameter.ParameterType);
	                    arguments.Add(argument);
					}
                    catch (Exception exception)
                    {
	                    Console.WriteLine(exception);
	                    throw;
                    }
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
	        return parameter.ParameterType.IsValueTypeOrNullableValueType();
        }

        private static bool IsRootPath(Uri requestUri, IOptions<UiServerOptions> options)
        {
            return requestUri.Segments.Length == 1 && requestUri.Segments[0] == "/" || 
                   requestUri.AbsolutePath == options.Value.HubPath;
        }
    }
}