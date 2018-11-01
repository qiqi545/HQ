// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

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