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

using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Extensions
{
	public static class IdentityErrorDescriberExtensions
	{
		public static IdentityError InvalidPhoneNumber(this IdentityErrorDescriber describer, string phoneNumber)
		{
			return new IdentityError
			{
				Code = nameof(InvalidPhoneNumber), Description = Resources.FormatInvalidPhoneNumber(phoneNumber)
			};
		}

		public static IdentityError DuplicatePhoneNumber(this IdentityErrorDescriber describer, string phoneNumber)
		{
			return new IdentityError
			{
				Code = nameof(DuplicatePhoneNumber), Description = Resources.FormatDuplicatePhoneNumber(phoneNumber)
			};
		}

		public static IdentityError MustHaveEmailPhoneOrUsername(this IdentityErrorDescriber describer)
		{
			return new IdentityError
			{
				Code = nameof(MustHaveEmailPhoneOrUsername), Description = ErrorStrings.MustHaveEmailPhoneOrUsername
			};
		}

		public static IdentityError InvalidTenantName(this IdentityErrorDescriber describer, string tenantName)
		{
			return new IdentityError
			{
				Code = nameof(InvalidTenantName), Description = Resources.FormatInvalidTenantName(tenantName)
			};
		}

		public static IdentityError DuplicateTenantName(this IdentityErrorDescriber describer, string tenantName)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateTenantName), Description = Resources.FormatDuplicateTenantName(tenantName)
			};
		}

		public static IdentityError InvalidApplicationName(this IdentityErrorDescriber describer,
			string applicationName)
		{
			return new IdentityError
			{
				Code = nameof(InvalidApplicationName),
				Description = Resources.FormatInvalidApplicationName(applicationName)
			};
		}

		public static IdentityError DuplicateApplicationName(this IdentityErrorDescriber describer,
			string applicationName)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateApplicationName),
				Description = Resources.FormatDuplicateApplicationName(applicationName)
			};
		}
	}
}