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
using System.Net.Http;
using HQ.Common;
using HQ.Platform.Api.Models;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Operations
{
    internal class OperationsMetaProvider : IMetaProvider
    {
        private readonly IOptions<OperationsApiOptions> _options;

        public OperationsMetaProvider(IOptions<OperationsApiOptions> options)
        {
            _options = options;
        }

        public void Populate(string baseUri, MetaCollection collection)
        {
            const string auth = "bearer";

            var versionString = typeof(OperationsMetaProvider).Assembly.GetName().Version.ToString();
            var options = _options.Value;

            var folder = new MetaFolder
            {
                name = "Operations",
                description = new MetaDescription
                {
                    content = "Provides diagnostic tools for server operators at runtime.",
                    type = Constants.MediaTypes.Markdown,
                    version = null
                },
                variable = new List<dynamic>(),
                item = new List<MetaItem>(),
                @event = new List<dynamic>(),
                auth = auth,
                protocolProfileBehavior = new { }
            };

            var rootPath = options.RootPath.TrimStart('/');

            if (options.EnableRouteDebugging)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Route Diagnostics",
                    Description = "Used to detect resolution issues in API path routing.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.RouteDebuggingPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }

            if (options.EnableOptionsDebugging)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Configuration Diagnostics",
                    Description = "Used to detect configuration binding errors and other issues with configuration, as well as inspect current values of all configurations.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.OptionsDebuggingPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }

            if (options.EnableEnvironmentEndpoint)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Environment Diagnostics",
                    Description = "Used to obtain diagnostic runtime information from the running node instance.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.EnvironmentEndpointPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }

            if (options.EnableServicesDebugging)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Services Diagnostics",
                    Description = "Used to detect errors in dependency injection (DI) or inversion of control (IoC) in the application container.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.ServicesDebuggingPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }

            if (options.EnableMetricsEndpoint)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Metrics Sample",
                    Description = "Used to sample all registered metrics in the system for reporting purposes.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.MetricsEndpointPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }

            if (options.EnableHealthChecksEndpoints)
            {
                if (!string.IsNullOrWhiteSpace(options.HealthChecksPath))
                {
                    var descriptor = new EndpointDescriptor
                    {
                        Auth = auth,
                        Name = "Health Checks (full)",
                        Description = "Used to monitor an API for its ability to respond to requests. This method checks all registered health checks for internal systems.",
                        Method = HttpMethod.Get,
                        Url = $"{baseUri}/{rootPath + options.HealthChecksPath}",
                        Version = versionString
                    };

                    folder.item.Add(MapFrom(descriptor));
                }

                if (!string.IsNullOrWhiteSpace(options.HealthCheckLivePath))
                {
                    var descriptor = new EndpointDescriptor
                    {
                        Auth = auth,
                        Name = "Health Check (live-only)",
                        Description = "Used to monitor an API for its ability to respond to requests. This method does not check internal systems.",
                        Method = HttpMethod.Get,
                        Url = $"{baseUri}/{rootPath + options.HealthCheckLivePath}",
                        Version = versionString
                    };

                    folder.item.Add(MapFrom(descriptor));
                }
            }

            if (options.EnableFeatureDebugging)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Feature Diagnostics",
                    Description = "Used to diagnose feature toggles, A/B testing, and cohorts.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.FeatureDebuggingPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }

            if (options.EnableCacheDebugging)
            {
                var descriptor = new EndpointDescriptor
                {
                    Auth = auth,
                    Name = "Cache Diagnostics",
                    Description = "Used to diagnose cache size, throughput, contention, and memory pressure.",
                    Method = HttpMethod.Get,
                    Url = $"{baseUri}/{rootPath + options.CacheDebuggingPath}",
                    Version = versionString
                };
                folder.item.Add(MapFrom(descriptor));
            }
            
            collection.item.Add(folder);

            collection.item.Sort();
        }

        private static MetaItem MapFrom(EndpointDescriptor descriptor)
        {
            var item = new MetaItem
            {
                id = Guid.NewGuid(),
                name = descriptor.Name,
                description = new MetaDescription
                {
                    content = descriptor.Description,
                    type = "text/markdown",
                    version = null
                },
                variable = new List<dynamic>(),
                @event = new List<dynamic>(),
                request = new MetaOperation
                {
                    url = MetaUrl.Raw(descriptor.Url),
                    auth = descriptor.Auth,
                    proxy = new { },
                    certificate = new { },
                    method = descriptor.Method.ToString(),
                    description = new MetaDescription
                    {
                        content = descriptor.Description,
                        type = "text/markdown",
                        version = descriptor.Version
                    },
                    header = new List<MetaParameter>
                    {
                        new MetaParameter
                        {
                            key = "Content-Type",
                            value = "application/json",
                            disabled = false,
                            description = "", /*new MetaDescription
                            {
                                content = "",
                                type = "text/markdown",
                                version = descriptor.Version
                            }*/
                        }
                    },
                    body = default(object)
                },
                response = new List<dynamic>(),
                protocolProfileBehavior = new { }
            };
            return item;
        }
    }
}
