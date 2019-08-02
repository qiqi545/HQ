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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using HQ.Common;
using HQ.Common.AspNetCore;
using HQ.Extensions.Options.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Operations
{
    internal static class OperationsMethods
    {
        public static OperationsReports.OptionsReport OptionsReport(IServiceProvider serviceProvider)
        {
            var report = new OperationsReports.OptionsReport();

            report.Options = typeof(IOptions<>).GetImplementationsOfOpenGeneric().GroupBy(x => x.Name).Select(x =>
            {
                // i.e., IOptions, IOptionsSnapshot, IOptionsMonitor, etc.
                var scope = x.Key.Substring(0, x.Key.Length - 2 /* `1 */);

                var values = x.Distinct()
                    .Where(t => !t.ContainsGenericParameters)
                    .Select(t =>
                    {
                        var valid = serviceProvider.TryBindOptions(t, false, out var options);

                        return new OperationsReports.OptionBindingReport
                        {
                            Type = t.GetInnerGenericTypeName(),
                            Value = options,
                            IsValid = valid
                        };
                    })
                    .OrderByDescending(v => !v.IsValid)
                    .ThenBy(v => v.Type)
                    .ToList();

                var hasErrors = false;
                foreach (var v in values)
                {
                    if (v.IsValid)
                    {
                        continue;
                    }

                    hasErrors = true;
                    break;
                }

                report.HasErrors |= hasErrors;

                return new OperationsReports.OptionReport {Scope = scope, HasErrors = hasErrors, Values = values};
            }).ToList();

            return report;
        }

        public static OperationsReports.ServicesReport ServicesReport(IServiceProvider serviceProvider)
        {
            var services = serviceProvider.GetRequiredService<IServiceCollection>();

            var missing = new HashSet<string>();
            var report = new OperationsReports.ServicesReport
            {
                MissingRegistrations = missing,
                Services = services.Select(x =>
                {
                    var serviceTypeName = x.ServiceType.Name;
                    var implementationTypeName = x.ImplementationType?.Name;
                    var implementationInstanceName = x.ImplementationInstance?.GetType().Name;

                    string implementationFactoryTypeName = null;
                    if (x.ImplementationFactory != null)
                    {
                        try
                        {
                            var result = x.ImplementationFactory.Invoke(serviceProvider);
                            if (result != null)
                            {
                                implementationFactoryTypeName = result.GetType().Name;
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            if (ex.Source == "Microsoft.Extensions.DependencyInjection.Abstractions")
                            {
                                var match = Regex.Match(ex.Message, "No service for type '([\\w.]*)'",
                                    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

                                if (match.Success)
                                {
                                    var typeName = match.Groups[1];
                                    missing.Add(typeName.Value);
                                }
                            }
                            else
                            {
                                Trace.TraceError($"{ex.ToString()}");
                            }
                        }
                    }

                    return new OperationsReports.ServiceReport
                    {
                        Lifetime = x.Lifetime,
                        ImplementationType = implementationTypeName,
                        ImplementationInstance = implementationInstanceName,
                        ImplementationFactory = implementationFactoryTypeName,
                        ServiceType = serviceTypeName
                    };
                }).ToList()
            };

            return report;
        }

        public static OperationsReports.HostedServicesReport HostedServicesReport(IServiceProvider serviceProvider)
        {
            var report = new OperationsReports.HostedServicesReport();
            var hostedServices = serviceProvider.GetServices<IHostedService>();

            foreach (var hostedService in hostedServices)
            {
                report.Services.Add(hostedService.GetType().Name);
            }
            
            return report;
        }
    }
}
