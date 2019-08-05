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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HQ.UI.Internal.UriTemplates;
using HQ.UI.Web.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.UI.Web
{
	public class HtmlSystem : UiSystem
	{
		private string _dom;
		private string _scripts;
		private string _styles;
		public StringBuilder Dom;
		public StringBuilder Scripts;

		public StringBuilder Styles;

		public string RenderDocument
		{
			get;
			private set;
		}

		public string RenderStyles => RenderDocument != null
			? throw new InvalidOperationException()
			: _styles ?? Styles?.ToString();

		public string RenderDom =>
			RenderDocument != null ? throw new InvalidOperationException() : _dom ?? Dom?.ToString();

		public string RenderScripts => RenderDocument != null
			? throw new InvalidOperationException()
			: _scripts ?? Scripts?.ToString();

		public override void Begin(UiContext context = null)
		{
			_styles = null;
			_dom = null;
			_scripts = null;
			RenderDocument = null;

			Styles = Pooling.StringBuilderPool.Get();
			Dom = Pooling.StringBuilderPool.Get();
			Scripts = Pooling.StringBuilderPool.Get();
		}

		public override void End()
		{
			_styles = RenderStyles;
			_dom = RenderDom;
			_scripts = RenderScripts;
			RenderDocument = RenderDocument;

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

		public override void PopulateAction(Ui ui, UiSettings settings, UiAction action, string template, object target,
			MethodInfo callee = null)
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
					foreach (var headerName in request.Headers.Keys)
						if (request.Headers.TryGetValue(headerName, out var value))
							parameters[headerName.Replace("-", string.Empty)] = value;

				//
				// Query Resolution (overwrites headers):
				foreach (var entry in request.Query)
					parameters[entry.Key] = entry.Value;

				//
				// Form Collection (overwrites queries):
				if (request.HasFormContentType && request.Form != null)
					foreach (var parameterName in request.Form.Keys)
						if (request.Form.TryGetValue(parameterName, out var value))
							parameters[parameterName] = value;
			}

			action.MethodName = callee?.Name ?? (IsRootPath(requestUri, options)
									? settings.DefaultPageMethodName
									: requestUri.Segments.LastOrDefault());

			PopulateArguments(ui, action, target, callee, parameters);
		}


		private static bool IsRootPath(Uri requestUri, IOptions<UiServerOptions> options)
		{
			return requestUri.Segments.Length == 1 &&
				   requestUri.Segments[0] == "/" ||
				   requestUri.AbsolutePath == options.Value.HubPath;
		}

		public void Document(string document)
		{
			RenderDocument = document;
		}

		public static async Task TryAuthenticateRequestAsync(ICustomAttributeProvider provider, UiContext context)
		{
			var httpContext = context.UiServices.GetRequiredService<IHttpContextAccessor>().HttpContext;
			if (httpContext == null)
				return;

			if (httpContext.Response.HasStarted)
				return;

			var schemeProvider = context.UiServices.GetService<IAuthenticationSchemeProvider>();
			if (schemeProvider == null)
				return;

			var declaredSchemeNames = Pooling.HashSetPool.Get();
			try
			{
				if (provider.HasAttribute<AuthorizeAttribute>())
					foreach (var authorize in provider.GetAttributes<AuthorizeAttribute>())
					{
						if (authorize?.AuthenticationSchemes == null)
						{
							declaredSchemeNames.Add("*");
							break;
						}

						foreach (var schemeName in authorize.AuthenticationSchemes.Split(new[] { "," },
							StringSplitOptions.RemoveEmptyEntries))
							declaredSchemeNames.Add(schemeName);
					}

				if (declaredSchemeNames.Count == 0)
					return;

				var authenticationSchemes = await schemeProvider.GetAllSchemesAsync();
				var availableSchemes = authenticationSchemes.OrderBy(x => declaredSchemeNames).ToList();
				if (availableSchemes.Count == 0)
					return;

				var anyScheme = declaredSchemeNames.Contains("*");

				AuthenticationScheme first = null;
				foreach (var scheme in availableSchemes)
				{
					if (!anyScheme && !declaredSchemeNames.Contains(scheme.Name))
						continue;
					first = first ?? scheme;
					var authenticated = await httpContext.AuthenticateAsync(scheme.Name);
					if (!authenticated.Succeeded)
						continue;
					httpContext.User = authenticated.Principal;
					return;
				}

				if (first == null)
					await httpContext.ForbidAsync();
				else
					await httpContext.ChallengeAsync(first.Name);
			}
			finally
			{
				Pooling.HashSetPool.Return(declaredSchemeNames);
			}
		}
	}
}