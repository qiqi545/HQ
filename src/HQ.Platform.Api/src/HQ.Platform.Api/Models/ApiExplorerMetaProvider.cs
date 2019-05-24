using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HQ.Common;
using HQ.Data.Contracts.Attributes;
using HQ.Platform.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Api.Models
{
    internal class ApiExplorerMetaProvider : IMetaProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _explorer;
        private readonly IOptions<PublicApiOptions> _options;

        public ApiExplorerMetaProvider(IApiDescriptionGroupCollectionProvider apiExplorer, IOptions<PublicApiOptions> options)
        {
            _explorer = apiExplorer;
            _options = options;
        }

        public void Populate(string baseUri, MetaCollection collection)
        {
            var descriptions = _explorer.ApiDescriptionGroups.Items.SelectMany(x => x.Items);

            var manifest = descriptions.GroupBy(x => ((ControllerActionDescriptor)x.ActionDescriptor).ControllerTypeInfo);

            foreach (var group in manifest)
            {
                var controllerName = ResolveControllerName(@group);

                var folder = new MetaFolder
                {
                    name = controllerName,
                    description = "",
                    variable = new List<dynamic>(),
                    item = new List<MetaItem>(),
                    @event = new List<dynamic>(),
                    auth = "bearer",
                    protocolProfileBehavior = new { }
                };

                foreach (var operation in @group.OrderBy(x => x.RelativePath).ThenBy(x => x.HttpMethod))
                {
                    var url = $"{baseUri}/{operation.RelativePath}";

                    var item = new MetaItem
                    {
                        id = Guid.NewGuid(),
                        name = operation.RelativePath,
                        description = "",
                        variable = new List<dynamic>(),
                        @event = new List<dynamic>(),
                        request = new
                        {
                            url,
                            auth = "bearer",
                            proxy = new { },
                            certificate = new { },
                            method = operation.HttpMethod,
                            description = new
                            {
                                content = "",
                                type = "text/markdown",
                                version = _options.Value.ApiVersion
                            },
                            header = new List<dynamic>
                            {
                                new
                                {
                                    disabled = false,
                                    description = new
                                    {
                                        content = "",
                                        type = "text/markdown",
                                        version = _options.Value.ApiVersion
                                    },
                                    key = "Content-Type",
                                    value = "application/json"
                                }
                            },
                            body = default(object)
                        },
                        response = new List<dynamic>(),
                        protocolProfileBehavior = new { }
                    };
                    folder.item.Add(item);
                }

                collection.item.Add(folder);
            }
        }

        private static string ResolveControllerName(IGrouping<TypeInfo, ApiDescription> @group)
        {
            var controllerType = group.Key;
            var controllerTypeName = controllerType.GetNonGenericName();

            if (!Attribute.IsDefined(controllerType, typeof(DescriptionAttribute)))
                return controllerTypeName.Replace(nameof(Controller), string.Empty);

            var description = (DescriptionAttribute) controllerType.GetCustomAttribute(typeof(DescriptionAttribute), true);
            return description.Description;
        }
    }
}
