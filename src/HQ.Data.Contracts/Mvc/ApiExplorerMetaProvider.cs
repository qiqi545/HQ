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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ActiveRoutes;
using HQ.Common.AspNetCore.Mvc;
using HQ.Common.Dates;
using HQ.Common.Models;
using HQ.Data.Contracts.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using TypeKitchen;

namespace HQ.Data.Contracts.Mvc
{
	public class ApiExplorerMetaProvider : IMetaProvider
	{
		private readonly IOptionsMonitor<AuthenticationOptions> _authenticationOptions;
		private readonly IApiDescriptionGroupCollectionProvider _explorer;
		private readonly IEnumerable<IMetaParameterProvider> _parameterProviders;
		private readonly IMetaVersionProvider _versionProvider;

		public ApiExplorerMetaProvider(
			IMetaVersionProvider versionProvider,
			IApiDescriptionGroupCollectionProvider apiExplorer,
			IEnumerable<IMetaParameterProvider> parameterProviders,
			IOptionsMonitor<AuthenticationOptions> authenticationOptions)
		{
			_versionProvider = versionProvider;
			_explorer = apiExplorer;
			_parameterProviders = parameterProviders;
			_authenticationOptions = authenticationOptions;
		}

		public void Populate(string baseUri, MetaCollection collection, IServiceProvider serviceProvider)
		{
			var foldersByType = new Dictionary<TypeInfo, MetaFolder>();
			var foldersByGroupName = new Dictionary<string, List<MetaFolder>>();

			// var rootVersion = _explorer.ApiDescriptionGroups.Version;

			var groupNames = _explorer.ApiDescriptionGroups.Items.Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
				.SelectMany(x => x.GroupName.Contains(",")
					? x.GroupName.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
					: new[] {x.GroupName}).Distinct().ToList();

			var groupFolders = groupNames.Select(x => new MetaFolder
			{
				name = x,
				description = new MetaDescription {content = "", type = "", version = null},
				variable = new List<dynamic>(),
				item = new List<MetaItem>(),
				@event = new List<dynamic>(),
				auth = null,
				protocolProfileBehavior = new { }
			}).ToDictionary(k => k.name, v => v);

			//
			// Map all folders:
			foreach (var descriptionGroup in _explorer.ApiDescriptionGroups.Items)
			{
				var groupName = descriptionGroup.GroupName;
				var orderedActions = descriptionGroup.Items.OrderBy(x => x.RelativePath).ThenBy(x => x.HttpMethod);

				foreach (var description in orderedActions)
				{
					var controllerDescriptor = (ControllerActionDescriptor) description.ActionDescriptor;
					var controllerTypeInfo = controllerDescriptor.ControllerTypeInfo;

					if (!ResolveControllerFeatureEnabled(controllerTypeInfo.AsType(), serviceProvider))
						continue;

					var controllerName = ResolveControllerName(controllerTypeInfo);

					var item = CreateOperationMetaItem(baseUri, description, serviceProvider);

					if (!foldersByType.TryGetValue(controllerTypeInfo, out var folder))
						foldersByType.Add(controllerTypeInfo,
							folder = new MetaFolder
							{
								name = controllerName,
								description = new MetaDescription
								{
									content = ResolveControllerDescription(controllerTypeInfo)?.Content,
									type = ResolveControllerDescription(controllerTypeInfo)?.MediaType,
									version = null
								},
								variable = new List<dynamic>(),
								item = new List<MetaItem>(),
								@event = new List<dynamic>(),
								auth = ResolveAuth(controllerDescriptor),
								protocolProfileBehavior = new { }
							});

					folder.item.Add(item);

					if (groupName == null)
						continue;

					var groups = groupName.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
					foreach (var group in groups)
					{
						if (!foldersByGroupName.TryGetValue(group, out var list))
							foldersByGroupName.Add(group, list = new List<MetaFolder>());
						if (list.Contains(folder))
							continue;
						list.Add(folder);
					}
				}
			}

			//
			// Create folder hierarchy:
			var roots = new List<MetaFolder>();
			var categories = new Dictionary<MetaCategoryAttribute, List<MetaFolder>>();
			foreach (var folder in foldersByType)
			{
				var controllerType = folder.Key;
				var category = ResolveControllerCategory(controllerType);
				if (category != null)
				{
					if (!categories.TryGetValue(category, out var list))
						categories.Add(category, list = new List<MetaFolder>());

					// Does this folder belong to a group?
					var inGroup = false;
					foreach (var groupName in groupNames)
					{
						if (!foldersByGroupName[groupName].Contains(folder.Value))
							continue;

						var groupFolder = groupFolders[groupName];
						groupFolder.item.Add(folder.Value);

						if (!list.Contains(groupFolder))
							list.Add(groupFolder);

						inGroup = true;
						break;
					}

					if (!inGroup)
						list.Add(folder.Value);
				}
				else
					roots.Add(folder.Value);
			}

			//
			// Add category folders:
			foreach (var entry in categories)
			{
				var category = entry.Key;

				var folderExists = collection.TryGetFolder(category.Name, out var categoryFolder);
				if (!folderExists)
				{
					categoryFolder = new MetaFolder
					{
						name = category.Name,
						description =
							new MetaDescription
							{
								content = category.Description, type = category.DescriptionMediaType, version = null
							},
						variable = new List<dynamic>(),
						item = new List<MetaItem>(),
						@event = new List<dynamic>(),
						protocolProfileBehavior = new { }
					};
					collection.item.Add(categoryFolder);
				}

				foreach (var subFolder in entry.Value.OrderBy(x => x.name))
					categoryFolder.item.Add(subFolder);

				categoryFolder.auth = ResolveCategoryAuth(categoryFolder);
			}

			//
			// Add root level folders:
			foreach (var folder in roots.OrderBy(x => x.name))
				collection.item.Add(folder);

			//
			// Change group name folder meta:
			foreach (var groupFolder in groupFolders)
			{
				var revisionName = groupFolder.Value.name;
				groupFolder.Value.name = $"Revision {revisionName}";

				if (_versionProvider.Enabled)
					foreach (var objectGroup in groupFolder.Value.item.OfType<MetaFolder>())
					foreach (var item in objectGroup.item)
					{
						item.request.url.query ??= (item.request.url.query = new List<MetaParameter>());
						item.request.url.query.Add(new MetaParameter
						{
							key = _versionProvider.VersionParameter,
							value = revisionName,
							description = "Sets the version revision number for this API request."
							// MetaDescription.PlainText("Sets the version revision number for this API request.")
						});
					}
			}
		}

		private MetaItem CreateOperationMetaItem(string baseUri, ApiDescription description,
			IServiceProvider serviceProvider)
		{
			var relativePath = Uri.UnescapeDataString(description.RelativePath ?? string.Empty);
			var url = $"{baseUri}/{relativePath}";
			var operation = new MetaOperation
			{
				auth = ResolveOperationAuth(description),
				proxy = new { },
				certificate = new { },
				method = description.HttpMethod,
				description = new MetaDescription {content = "", type = MediaTypeNames.Text.Markdown, version = null},
				header = new List<MetaParameter>
				{
					new MetaParameter
					{
						disabled = false,
						description = "", /* new MetaDescription
                            {
                                content = "",
                                type = MediaTypeNames.Markdown,
                                version = null
                            },*/
						key = HeaderNames.ContentType,
						value = MediaTypeNames.Application.Json
					}
				},
				body = default
			};

			operation.url = MetaUrl.FromRaw(url);

			foreach (var provider in _parameterProviders)
				provider.Enrich(url, operation, serviceProvider);

			var item = new MetaItem
			{
				id = Guid.NewGuid(),
				name = relativePath,
				description =
					new MetaDescription {content = "", type = MediaTypeNames.Text.Markdown, version = null},
				variable = new List<dynamic>(),
				@event = new List<dynamic>(),
				request = operation,
				response = new List<dynamic>(),
				protocolProfileBehavior = new { }
			};

			if (description.SupportedRequestFormats.Count > 0)
			{
				var bodyParameter = description.ParameterDescriptions.SingleOrDefault(
					x => x.Source.IsFromRequest && x.Source.Id == "Body");

				//
				// Token Capture:
				if (item.request.method == "POST" && bodyParameter?.Type?.Name == "BearerTokenRequest")
				{
					item.@event.Add(new
					{
						listen = "test",
						script = new
						{
							id = "66a87d23-bc0e-432c-acee-cb48d3704947",
							exec = new List<string>
							{
								"var data = JSON.parse(responseBody);\r",
								"postman.setGlobalVariable(\"accessToken\", data.accessToken);"
							},
							type = "text/javascript"
						}
					});
					item.request.body = new
					{
						mode = "raw",
						raw =
							"{\r\n\t\"IdentityType\": \"Username\",\r\n\t\"Identity\": \"\",\r\n\t\"Password\": \"\"\r\n}"
					};
				}

				//
				// Body Definition (roots only):
				if (bodyParameter != null && bodyParameter.Type != null &&
				    !typeof(IEnumerable).IsAssignableFrom(bodyParameter.Type))
					item.request.body = new
					{
						mode = "raw",
						raw = JsonConvert.SerializeObject(
							FormatterServices.GetUninitializedObject(bodyParameter.Type))
					};
			}

			//
			// Bearer:
			if (item.request?.auth != null &&
			    (item.request.auth.Equals("bearer", StringComparison.OrdinalIgnoreCase) ||
			     item.request.auth.Equals("platformbearer", StringComparison.OrdinalIgnoreCase)))
				item.request.header.Add(new MetaParameter
				{
					key = "Authorization",
					value = "Bearer {{accessToken}}",
					description = "Access Token",
					type = "text"
				});

			return item;
		}

		private static string ResolveCategoryAuth(MetaFolder categoryFolder)
		{
			var categoryAuth = categoryFolder.item.Where(x => !string.IsNullOrWhiteSpace(x.request?.auth))
				.Select(x => x.request.auth).Distinct();

			return string.Join(",", categoryAuth);
		}

		private string ResolveOperationAuth(ApiDescription operation)
		{
			return ResolveAuth(operation.ActionDescriptor);
		}

		private string ResolveAuth(ActionDescriptor descriptor)
		{
			var authorize =
				descriptor.EndpointMetadata.FirstOrDefault(x => x is AuthorizeAttribute) as IAuthorizeData ??
				descriptor.EndpointMetadata.FirstOrDefault(x => x is DynamicControllerAttribute) as IAuthorizeData;

			if (authorize == null)
				return null;

			return !string.IsNullOrWhiteSpace(authorize.AuthenticationSchemes)
				? authorize.AuthenticationSchemes.ToLowerInvariant()
				: (_authenticationOptions.CurrentValue.DefaultChallengeScheme ??
				   _authenticationOptions.CurrentValue.DefaultScheme)
				.ToLowerInvariant();
		}

		private static MetaCategoryAttribute ResolveControllerCategory(MemberInfo controllerType)
		{
			return !Attribute.IsDefined(controllerType, typeof(MetaCategoryAttribute))
				? null
				: (MetaCategoryAttribute) controllerType.GetCustomAttribute(typeof(MetaCategoryAttribute), true);
		}

		private static bool ResolveControllerFeatureEnabled(Type controllerType, IServiceProvider serviceProvider)
		{
			var componentTypes = serviceProvider.GetServices<IDynamicComponent>().Select(x => x.GetType()).ToList();
			foreach (var @interface in controllerType.GetInterfaces())
			{
				if (!@interface.IsGenericType)
					continue;
				if (@interface.GetGenericTypeDefinition() != typeof(IDynamicComponentEnabled<>))
					continue;
				if (!componentTypes.Contains(@interface.GenericTypeArguments[0]))
					return false; // requires an installed component that isn't present
			}

			var attribute = !Attribute.IsDefined(controllerType, typeof(DynamicControllerAttribute))
				? null
				: (DynamicControllerAttribute) controllerType.GetCustomAttribute(typeof(DynamicControllerAttribute), true);

			if (attribute == null)
				return true;

			// FIXME: can't "see" enabled flag from this layer anymore!
			attribute.ServiceProvider = serviceProvider;
			//return attribute.Enabled;
			return true;
		}

		private static MetaDescriptionAttribute ResolveControllerDescription(MemberInfo controllerType)
		{
			if (!Attribute.IsDefined(controllerType, typeof(MetaDescriptionAttribute)))
				return null;
			var description =
				(MetaDescriptionAttribute) controllerType.GetCustomAttribute(typeof(MetaDescriptionAttribute), true);
			return description;
		}

		private static string ResolveControllerName(Type controllerType)
		{
			var controllerTypeName = controllerType.GetNonGenericName();

			if (!Attribute.IsDefined(controllerType, typeof(DisplayNameAttribute)))
				return controllerTypeName.Replace(nameof(Controller), string.Empty);

			var description =
				(DisplayNameAttribute) controllerType.GetCustomAttribute(typeof(DisplayNameAttribute), true);
			return description.DisplayName;
		}
	}
}