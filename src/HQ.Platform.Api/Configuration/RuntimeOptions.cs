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
	public class RuntimeOptions : FeatureToggle, IProtectedFeature, IComponentOptions
	{
		public string Scheme { get; set; } = Constants.Security.Schemes.PlatformBearer;
		public string Policy { get; set; } = Constants.Security.Policies.ManageObjects;
		public string RootPath { get; set; } = "/api";
		public bool CreateIfNotExists { get; set; } = true;
		public bool MigrateOnStartup { get; set; } = true;
		public bool EnableRest { get; set; } = true;
		public bool EnableGraphQl { get; set; } = true;
	}
}