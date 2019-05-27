using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HQ.Common;
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
            var foldersByType = new Dictionary<TypeInfo, MetaFolder>();
            var foldersByGroupName = new Dictionary<string, List<MetaFolder>>();

            var rootVersion = _explorer.ApiDescriptionGroups.Version;

            var groupNames = _explorer.ApiDescriptionGroups.Items.Where(x => !string.IsNullOrWhiteSpace(x.GroupName))
                .SelectMany(x => x.GroupName.Contains(",") ? x.GroupName.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
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

            // Map all folders:
            foreach (var descriptionGroup in _explorer.ApiDescriptionGroups.Items)
            {
                var groupName = descriptionGroup.GroupName;

                foreach (var description in descriptionGroup.Items.OrderBy(x => x.RelativePath).ThenBy(x => x.HttpMethod))
                {
                    var controllerDescriptor = (ControllerActionDescriptor) description.ActionDescriptor;
                    var controllerType = controllerDescriptor.ControllerTypeInfo;
                    var controllerName = ResolveControllerName(controllerType);

                    var item = CreateOperationMetaItem(baseUri, description);

                    if (!foldersByType.TryGetValue(controllerType, out var folder))
                    {
                        foldersByType.Add(controllerType, folder = new MetaFolder
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

                    folder.item.Add(item);

                    if (groupName == null)
                        continue;

                    var groups = groupName.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var group in groups)
                    {
                        if(!foldersByGroupName.TryGetValue(group, out var list))
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
                        {
                            list.Add(groupFolder);
                        }

                        inGroup = true;
                        break;
                    }

                    if(!inGroup)
                        list.Add(folder.Value);
                }
                else
                {
                    roots.Add(folder.Value);
                }
            }

            //
            // Add category folders:
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

                categoryFolder.auth = ResolveCategoryAuth(categoryFolder);

                collection.item.Add(categoryFolder);
            }

            //
            // Add root level folders:
            foreach (var folder in roots.OrderBy(x => x.name))
            {
                collection.item.Add(folder);
            }

            //
            // Change group name folder meta:
            foreach (var groupFolder in groupFolders)
            {
                var name = groupFolder.Value.name;
                groupFolder.Value.name = $"Revision {name}";
            }
        }
        
        private MetaItem CreateOperationMetaItem(string baseUri, ApiDescription description)
        {
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
                            key = Constants.HttpHeaders.ContentType,
                            value = Constants.MediaTypes.Json
                        }
                    },
                    body = default
                },
                response = new List<dynamic>(),
                protocolProfileBehavior = new { }
            };
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
