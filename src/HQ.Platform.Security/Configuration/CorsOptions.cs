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

namespace HQ.Platform.Security.Configuration
{
    public class CorsOptions : FeatureToggle
    {
        public string[] Origins { get; set; }
        public string[] Methods { get; set; }
        public string[] Headers { get; set; }
        public string[] ExposedHeaders { get; set; }
        public bool AllowCredentials { get; set; } = true;
        public bool AllowOriginWildcards { get; set; } = true;
        public int? PreflightMaxAgeSeconds { get; set; } = null;

        public CorsOptions() : this(false) { }

        public CorsOptions(bool forBinding)
        {
            // IConfiguration.Bind adds to existing arrays...
            if (forBinding)
                return;

            Origins = new[] { "*" };
            Methods = new[] { "*" };
            Headers = new[] { "*" };
            ExposedHeaders = new string[] { };
        }
    }
}
