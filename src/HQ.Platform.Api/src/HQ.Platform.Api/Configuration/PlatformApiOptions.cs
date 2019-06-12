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

using System.Reflection;
using HQ.Extensions.Scheduling.Configuration;

namespace HQ.Platform.Api.Configuration
{
    public class PlatformApiOptions
    {
        public string ApiName { get; set; } = Assembly.GetExecutingAssembly().GetName()?.Name;
        public string ApiVersion { get; set; } = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();

        public RequestLimitOptions RequestLimits { get; set; } = new RequestLimitOptions();
        public JsonConversionOptions JsonConversion { get; set; } = new JsonConversionOptions();
        public MethodOverrideOptions MethodOverrides { get; set; } = new MethodOverrideOptions();
        public ResourceRewritingOptions ResourceRewriting { get; set; } = new ResourceRewritingOptions();
        public VersioningOptions Versioning { get; set; } = new VersioningOptions();
        public MultiTenancyOptions MultiTenancy { get; set; } = new MultiTenancyOptions();
        public CanonicalRoutesOptions CanonicalRoutes { get; set; } = new CanonicalRoutesOptions();
        public BackgroundTaskOptions BackgroundTasks { get; set; } = new BackgroundTaskOptions();
    }
}
