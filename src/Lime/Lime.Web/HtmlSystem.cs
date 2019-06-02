// Copyright (c) Daniel Crenna & Contributor. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Lime.Internal;
using Lime.Internal.UriTemplates;
using Lime.Web.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TypeKitchen;

namespace Lime.Web
{
    public class HtmlSystem : UiSystem
    {
	    private string _styles;
	    private string _dom;
	    private string _scripts;
	    private string _document;

		public StringBuilder Styles;
		public StringBuilder Dom;
		public StringBuilder Scripts;
		
		public string RenderDocument => _document;
		public string RenderStyles => _document != null ? throw new InvalidOperationException() : _styles ?? Styles?.ToString();
		public string RenderDom => _document != null ? throw new InvalidOperationException() : _dom ?? Dom?.ToString();
        public string RenderScripts => _document != null ? throw new InvalidOperationException() : _scripts ?? Scripts?.ToString();
        
		public override void Begin(UiContext context = null)
		{
			_styles = null;
            _dom = null;
            _scripts = null;
            _document = null;

            Styles = Pooling.StringBuilderPool.Get();
            Dom = Pooling.StringBuilderPool.Get();
            Scripts = Pooling.StringBuilderPool.Get();
        }

        public override void End()
        {
	        _styles = RenderStyles;
            _dom = RenderDom;
            _scripts = RenderScripts;

            Pooling.StringBuilderPool.Return(Styles);
			Pooling.StringBuilderPool.Return(Dom);
            Pooling.StringBuilderPool.Return(Scripts);
        }
		
		public virtual string ScriptsSection()
        {
            return "<!-- SCRIPTS -->";
        }

        public virtual string StylesSection()
        {
            return "<!-- STYLES -->";
        }

        public override void PopulateAction(Ui ui, UiSettings settings, UiAction action, string template, object target, MethodInfo callee = null)
        {
			var http = ui.Context.UiServices.GetRequiredService<IHttpContextAccessor>();
            var options = ui.Context.UiServices.GetRequiredService<IOptions<UiServerOptions>>();

			var request = http.HttpContext.Request;
			var requestUri = new Uri(request.GetEncodedUrl(), UriKind.Absolute);
			
			var uriTemplate = new UriTemplate(template, caseInsensitiveParameterNames: true);
            
            //
            // URI Resolution:
            var templateParameters = uriTemplate.GetParameters(requestUri);
            var parameters = templateParameters ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (ui.Context is WebUiContext context)
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
            action.MethodName = callee?.Name ?? (IsRootPath(requestUri, options) ? settings.DefaultPageMethodName : requestUri.Segments.LastOrDefault());
             
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

            var arguments = Pooling.ListPool<object>.Get();
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
		                         $"No UI instance passed to PopulateAction or found in this synchronization context");

	                    arguments.Add(ui);
                        continue;
                    }

                    if (NotResolvableByContainer(parameter))
                    {
	                    arguments.Add(parameter.ParameterType.IsValueType
		                    ? Instancing.CreateInstance(parameter.ParameterType)
		                    : null);
                        continue;
                    }

                    try
                    {
	                    var argument = ui.Context.UiServices.GetService(parameter.ParameterType);
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
                Pooling.ListPool<object>.Return(arguments);
            }
        }

        private static bool NotResolvableByContainer(ParameterInfo parameter)
        {
	        return parameter.ParameterType.IsValueTypeOrNullableValueType();
        }

        private static bool IsRootPath(Uri requestUri, IOptions<UiServerOptions> options)
        {
	        return requestUri.Segments.Length == 1 &&
	               requestUri.Segments[0] == "/" ||
	               requestUri.AbsolutePath == options.Value.HubPath;
        }

        public void Document(string document)
        {
	        _document = document;
        }
    }
}