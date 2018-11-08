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

using HQ.Common;
using HQ.Common.Configuration;

namespace HQ.Domicile.Configuration
{
    public class PublicApiOptions
    {
        public RequestLimitOptions RequestLimits { get; set; } = new RequestLimitOptions();
        public JsonMultiCaseOptions JsonMultiCase { get; set; } = new JsonMultiCaseOptions();
        public MethodOverrideOptions MethodOverrides { get; set; } = new MethodOverrideOptions();
        public ResourceRewritingOptions ResourceRewriting { get; set; } = new ResourceRewritingOptions();

        public class RequestLimitOptions : FeatureToggle<PublicApiOptions>
        {
            public long MaxRequestSizeBytes { get; set; } = 30_000_000;
        }

        public class JsonMultiCaseOptions : FeatureToggle<PublicApiOptions>
        {
            public string QueryStringParameter { get; set; } = HqQueryStrings.MultiCase;
        }

        public class MethodOverrideOptions : FeatureToggle<PublicApiOptions>
        {
            public string MethodOverrideHeader { get; set; } = HttpHeaders.MethodOverride;
            public string[] AllowedMethodOverrides { get; set; } = {HttpVerbs.Delete, HttpVerbs.Head, HttpVerbs.Put};
        }

        public class ResourceRewritingOptions : FeatureToggle<PublicApiOptions>
        {
            public string ActionHeader { get; set; } = HttpHeaders.Action;
        }
    }
}
