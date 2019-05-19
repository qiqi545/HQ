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


using System.Net;
using HQ.Common;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Configuration
{
    public class VersioningOptions : FeatureToggle
    {
        public bool RequireExplicitVersion { get; set; } = true;
        public int ExplicitVersionRequiredStatusCode = (int) HttpStatusCode.NotFound;

        public bool EnableVersionHeader { get; set; } = false;
        public string VersionHeader { get; set; } = Constants.Versioning.VersionHeader;

        public bool EnableVersionParameter { get; set; } = true;
        public string VersionParameter { get; set; } = Constants.Versioning.VersionParameter;

        public bool EnableVersionPath { get; set; } = true;
        public string VersionPathPrefix { get; set; } = Constants.Versioning.VersionPathPrefix;

        public bool EnableUserVersions { get; set; } = true;
        public string UserVersionClaim { get; set; } = Constants.Versioning.UserVersionClaim;

        public int? VersionLifetimeSeconds { get; set; } = null;

        public string[] VersionAgnosticPaths { get; set; } = {"/"};
    }
}
