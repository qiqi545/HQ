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

namespace HQ.Platform.Api.Configuration
{
    public class JsonConversionOptions : FeatureToggle
    {
        public string MultiCaseOperator { get; set; } = Constants.QueryStrings.MultiCase;

        public string EnvelopeOperator { get; set; } = Constants.QueryStrings.Envelope;
        public bool EnvelopeEnabled { get; set; } = false;

        public string TrimOperator { get; set; } = Constants.QueryStrings.Trim;
        public bool TrimEnabled { get; set; } = true;

        public string PrettyPrintOperator { get; set; } = Constants.QueryStrings.PrettyPrint;
        public bool PrettyPrintEnabled { get; set; } = true;
    }
}
