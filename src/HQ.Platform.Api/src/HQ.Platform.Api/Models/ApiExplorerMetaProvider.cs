using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HQ.Common;
using HQ.Data.Contracts.Attributes;
using HQ.Platform.Api.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Models
{
    internal class ApiExplorerMetaProvider : IMetaProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _explorer;
        private readonly IServiceProvider _serviceProvider;

        public ApiExplorerMetaProvider(IApiDescriptionGroupCollectionProvider apiExplorer, IServiceProvider serviceProvider)
        {
            _explorer = apiExplorer;
            _serviceProvider = serviceProvider;
        }

        public void Populate(string baseUri, MetaCollection collection)
        {
            var descriptions = _explorer.ApiDescriptionGroups.Items.SelectMany(x => x.Items);
            var endpointsByController = descriptions.GroupBy(x => ((ControllerActionDescriptor)x.ActionDescriptor));

            var allFolders = new Dictionary<TypeInfo, MetaFolder>();
            
            var rootVersion = _explorer.ApiDescriptionGroups.Version;

            //
            // Map all folders:
            foreach (ApiDescriptionGroup descriptionGroup in _explorer.ApiDescriptionGroups.Items)
            {
                var groupName = descriptionGroup.GroupName;

                foreach (ApiDescription description in descriptionGroup.Items.OrderBy(x => x.RelativePath).ThenBy(x => x.HttpMethod))
                {
                    var controllerDescriptor = (ControllerActionDescriptor) description.ActionDescriptor;
                    var controllerType = controllerDescriptor.ControllerTypeInfo;
                    var controllerName = ResolveControllerName(controllerType);

                    if (!allFolders.TryGetValue(controllerType, out var folder))
                    {
                        allFolders.Add(controllerType, folder = new MetaFolder
                        {
                            name = controllerName,
                            description = new MetaDescription
                            {
                                content = ResolveControllerDescription(controllerType)?.Content,
                                type = ResolveControllerDescription(controllerType)?.MediaType,
                                version = null
                            },
                            variable = new List<dynamic>(),
                            item = new List<MetaItem>(),
                            @event = new List<dynamic>(),
                            auth = ResolveAuth(controllerDescriptor),
                            protocolProfileBehavior = new { }
                        });
                    }

                    var url = $"{baseUri}/{description.RelativePath}";
                    var item = new MetaItem
                    {
                        id = Guid.NewGuid(),
                        name = description.RelativePath,
                        description = new MetaDescription
                        {
                            content = "",
                            type = Constants.MediaTypes.Markdown,
                            version = null
                        },
                        variable = new List<dynamic>(),
                        @event = new List<dynamic>(),
                        request = new MetaOperation
                        {
                            url = url,
                            auth = ResolveOperationAuth(description),
                            proxy = new { },
                            certificate = new { },
                            method = description.HttpMethod,
                            description = new MetaDescription
                            {
                                content = "",
                                type = Constants.MediaTypes.Markdown,
                                version = null
                            },
                            header = new List<MetaHeader>
                            {
                                new MetaHeader
                                {
                                    disabled = false,
                                    description = new MetaDescription
                                    {
                                        content = "",
                                        type = Constants.MediaTypes.Markdown,
                                        version = null
                                    },
                                    key = "Content-Type",
                                    value = "application/json"
                                }
                            },
                            body = default
                        },
                        response = new List<dynamic>(),
                        protocolProfileBehavior = new { }
                    };

                    folder.item.Add(item);
                }
            }

            //
            // Create folder hierarchy:
            var roots = new List<MetaFolder>();
            var categories = new Dictionary<MetaCategoryAttribute, List<MetaFolder>>();
            foreach (var folder in allFolders)
            {
                var controllerType = folder.Key;
                var category = ResolveControllerCategory(controllerType);

                if (category != null)
                {
                    if (!categories.TryGetValue(category, out var list))
                        categories.Add(category, list = new List<MetaFolder>());
                    list.Add(folder.Value);
                }
                else
                {
                    roots.Add(folder.Value);
                }
            }

            //
            // Add folder tree:
            foreach (var entry in categories)
            {
                var category = entry.Key;
                var categoryFolder = new MetaFolder
                {
                    name = category.Name,
                    description = new MetaDescription
                    {
                        content = category.Description,
                        type = category.DescriptionMediaType,
                        version = null
                    },
                    variable = new List<dynamic>(),
                    item = new List<MetaItem>(),
                    @event = new List<dynamic>(),
                    protocolProfileBehavior = new { }
                };
                foreach (var subFolder in entry.Value.OrderBy(x => x.name))
                    categoryFolder.item.Add(subFolder);
                var categoryAuth = categoryFolder.item.Where(x => !string.IsNullOrWhiteSpace(x.request?.auth)).Select(x => x.request.auth).Distinct();
                categoryFolder.auth = string.Join(",", categoryAuth);
                collection.item.Add(categoryFolder);
            }

            //
            // Add root level folders:
            foreach (var folder in roots.OrderBy(x => x.name))
            {
                collection.item.Add(folder);
            }
        }

        private string ResolveOperationAuth(ApiDescription operation)
        {
            return ResolveAuth(operation.ActionDescriptor);
        }
        
        private string ResolveAuth(ActionDescriptor descriptor)
        {
            if (!(descriptor.EndpointMetadata.FirstOrDefault(x => x is AuthorizeAttribute) is AuthorizeAttribute authAttribute))
                return null;

            if (!string.IsNullOrWhiteSpace(authAttribute.AuthenticationSchemes))
                return authAttribute.AuthenticationSchemes.ToLowerInvariant();

            var options = _serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>();
            return (options.Value.DefaultChallengeScheme ?? options.Value.DefaultScheme).ToLowerInvariant();
        }

        private static MetaCategoryAttribute ResolveControllerCategory(MemberInfo controllerType)
        {
            return !Attribute.IsDefined(controllerType, typeof(MetaCategoryAttribute)) ? null
                : (MetaCategoryAttribute) controllerType.GetCustomAttribute(typeof(MetaCategoryAttribute), true);
        }

        private static MetaDescriptionAttribute ResolveControllerDescription(MemberInfo controllerType)
        {
            if (!Attribute.IsDefined(controllerType, typeof(MetaDescriptionAttribute)))
                return null;
            var description = (MetaDescriptionAttribute)controllerType.GetCustomAttribute(typeof(MetaDescriptionAttribute), true);
            return description;
        }

        private static string ResolveControllerName(Type controllerType)
        {
            var controllerTypeName = controllerType.GetNonGenericName();

            if (!Attribute.IsDefined(controllerType, typeof(DisplayNameAttribute)))
                return controllerTypeName.Replace(nameof(Controller), string.Empty);

            var description = (DisplayNameAttribute) controllerType.GetCustomAttribute(typeof(DisplayNameAttribute), true);
            return description.DisplayName;
        }
    }
}
