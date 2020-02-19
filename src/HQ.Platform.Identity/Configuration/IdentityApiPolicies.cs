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

using ActiveRoutes;
using Constants = HQ.Common.Constants;

namespace HQ.Platform.Identity.Configuration
{
	public class IdentityApiPolicies
	{
		public ManageUsersPolicy Users { get; set; } = new ManageUsersPolicy();
		public ManageRolesPolicy Roles { get; set; } = new ManageRolesPolicy();
		public ManageApplicationsPolicy Applications { get; set; } = new ManageApplicationsPolicy();
		public ManageTenantsPolicy Tenants { get; set; } = new ManageTenantsPolicy();

		public class ManageUsersPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; } = Constants.Security.Schemes.PlatformBearer;
			public string Policy { get; set; } = Constants.Security.Policies.ManageUsers;
		}

		public class ManageRolesPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; } = Constants.Security.Schemes.PlatformBearer;
			public string Policy { get; set; } = Constants.Security.Policies.ManageRoles;
		}

		public class ManageApplicationsPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; } = Constants.Security.Schemes.PlatformBearer;
			public string Policy { get; set; } = Constants.Security.Policies.ManageApplications;
		}

		public class ManageTenantsPolicy : IFeatureScheme, IFeaturePolicy
		{
			public string Scheme { get; set; } = Constants.Security.Schemes.PlatformBearer;
			public string Policy { get; set; } = Constants.Security.Policies.ManageTenants;
		}
	}
}