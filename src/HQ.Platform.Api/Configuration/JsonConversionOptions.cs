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
using System.Linq;
using HQ.Common;
using HQ.Common.Models;
using HQ.Common.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Api.Configuration
{
    public class JsonConversionOptions : FeatureToggle, IMetaParameterProvider
    {
	    public string MultiCaseOperator { get; set; } = Constants.QueryStrings.MultiCase;
        public string EnvelopeOperator { get; set; } = Constants.QueryStrings.Envelope;
        public string TrimOperator { get; set; } = Constants.QueryStrings.Trim;
        public string PrettyPrintOperator { get; set; } = Constants.QueryStrings.PrettyPrint;

        public void Enrich(string url, MetaOperation operation, IServiceProvider serviceProvider)
        {
	        if (operation.url == null)
		        operation.url = MetaUrl.FromRaw(url);

			if (Enabled)
	        {
		        var transforms = serviceProvider.GetServices<ITextTransform>();
		        var cases = transforms.Select(x => x.Name.ToLowerInvariant()).ToList();

		        var multiCaseParameter = new MetaParameter
		        {
			        key = MultiCaseOperator,
			        value = cases.FirstOrDefault() ?? string.Empty,
			        description = $"Transforms responses to alternative cases. Valid values are: {string.Join(", ", cases)}.",
			        disabled = true
		        };

				var envelopeParameter = new MetaParameter
		        {
			        key = EnvelopeOperator,
			        value = "1",
			        description = "Transforms responses to include more information in the payload for constrained clients.",
			        disabled = true
		        };

				var prettyPrintParameter = new MetaParameter
		        {
			        key = PrettyPrintOperator,
			        value = "1",
			        description = "Enhances readability of responses by adding whitespace and nesting.",
			        disabled = true
		        };

				var trimParameter = new MetaParameter
				{
					key = TrimOperator,
					value = "1",
					description = "Reduces response weight by omitting null and default values.",
					disabled = true
				};

				operation.url.query = operation.url.query ?? (operation.url.query = new List<MetaParameter>());
				operation.url.query.AddRange(new []
				{
					multiCaseParameter,
					envelopeParameter,
					trimParameter,
					prettyPrintParameter
				});
	        }
        }
    }
}
