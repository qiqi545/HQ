using System;
using System.Collections.Generic;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Controllers;
using HQ.Platform.Api.Models;
using HQ.Platform.Operations.Configuration;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Operations.Models
{
    internal class DevOpsMetaProvider : IMetaProvider
    {
        private readonly IOptions<DevOpsApiOptions> _options;
        private readonly IOptions<PublicApiOptions> _api;

        public DevOpsMetaProvider(IOptions<DevOpsApiOptions> options, IOptions<PublicApiOptions> api)
        {
            _options = options;
            _api = api;
        }

        public void Populate(string baseUri, MetaCollection collection)
        {
            var options = _options.Value;
            var api = _api.Value;

            var folder = new
            {
                name = options.RootPath,
                description = "",
                variable = new List<dynamic>(),
                item = new List<dynamic>(),
                @event = new List<dynamic>(),
                auth = "bearer",
                protocolProfileBehavior = new { }
            };

            if (options.EnableRouteDebugging)
            {
                var path = options.RouteDebuggingPath.TrimStart('/');

                var item = new
                {
                    id = Guid.NewGuid(),
                    name = path,
                    description = "",
                    variable = new List<dynamic>(),
                    @event = new List<dynamic>(),
                    request = new
                    {
                        url = $"{baseUri}/{path}",
                        auth = "bearer",
                        proxy = new { },
                        certificate = new { },
                        method = "GET",
                        description = new
                        {
                            content = "",
                            type = "text/markdown",
                            version = api.ApiVersion
                        },
                        header = new List<dynamic>
                        {
                            new
                            {
                                key = "Content-Type",
                                value = "application/json",
                                disabled = false,
                                description = new
                                {
                                    content = "",
                                    type = "text/markdown",
                                    version = api.ApiVersion
                                },
                            }
                        },
                        body = default(object)
                    },
                    response = new List<dynamic>(),
                    protocolProfileBehavior = new { }
                };
                folder.item.Add(item);
            }

            if (options.EnableOptionsDebugging)
            {
                var path = options.OptionsDebuggingPath.TrimStart('/');

                var item = new
                {
                    id = Guid.NewGuid(),
                    name = path,
                    description = "",
                    variable = new List<dynamic>(),
                    @event = new List<dynamic>(),
                    request = new
                    {
                        url = $"{baseUri}/{path}",
                        auth = "bearer",
                        proxy = new { },
                        certificate = new { },
                        method = "GET",
                        description = new
                        {
                            content = "",
                            type = "text/markdown",
                            version = api.ApiVersion
                        },
                        header = new List<dynamic>
                        {
                            new
                            {
                                key = "Content-Type",
                                value = "application/json",
                                disabled = false,
                                description = new
                                {
                                    content = "",
                                    type = "text/markdown",
                                    version = api.ApiVersion
                                },
                            }
                        },
                        body = default(object)
                    },
                    response = new List<dynamic>(),
                    protocolProfileBehavior = new { }
                };
                folder.item.Add(item);
            }

            if (options.EnableEnvironmentEndpoint)
            {
                var path = options.EnvironmentEndpointPath.TrimStart('/');

                var item = new
                {
                    id = Guid.NewGuid(),
                    name = path,
                    description = "",
                    variable = new List<dynamic>(),
                    @event = new List<dynamic>(),
                    request = new
                    {
                        url = $"{baseUri}/{path}",
                        auth = "bearer",
                        proxy = new { },
                        certificate = new { },
                        method = "GET",
                        description = new
                        {
                            content = "",
                            type = "text/markdown",
                            version = api.ApiVersion
                        },
                        header = new List<dynamic>
                        {
                            new
                            {
                                key = "Content-Type",
                                value = "application/json",
                                disabled = false,
                                description = new
                                {
                                    content = "",
                                    type = "text/markdown",
                                    version = api.ApiVersion
                                },
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
}
