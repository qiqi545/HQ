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
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using HQ.Common;
using Microsoft.IdentityModel.Tokens;

namespace HQ.Platform.Security.Configuration
{
    public class SecurityOptions
    {
        public ClaimOptions Claims { get; set; } = new ClaimOptions();
        public TokenOptions Tokens { get; set; } = new TokenOptions();
        public HttpsOptions Https { get; set; } = new HttpsOptions();
        public BlockListOptions BlockLists { get; set; } = new BlockListOptions();
        public WebServerOptions WebServer { get; set; } = new WebServerOptions();
        public CorsOptions Cors { get; set; }
        public CookieOptions Cookies { get; set; } = new CookieOptions();

		[NotMapped, IgnoreDataMember]
		internal SigningCredentials Signing { get; set; }

		[NotMapped, IgnoreDataMember]
		internal EncryptingCredentials Encrypting { get; set; }

        public SecurityOptions() : this(false) { }

        public SecurityOptions(bool forBinding = false)
        {
            Cors = new CorsOptions(forBinding);
        }
    }
}
