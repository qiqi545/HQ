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
    public class TokenOptions : FeatureToggle
    {
        public string Path { get; set; } = "tokens";
        public string SigningKey { get; set; } = Constants.Tokens.NoSigningKeySet;
        public string EncryptionKey { get; set; } = Constants.Tokens.NoEncryptionKeySet;
        public string Issuer { get; set; } = "https://mysite.com";
        public string Audience { get; set; } = "https://mysite.com";
        public int TimeToLiveSeconds { get; set; } = 180;
        public bool Encrypt { get; set; } = true;
        public int ClockSkewSeconds { get; set; } = 10;
    }
}
