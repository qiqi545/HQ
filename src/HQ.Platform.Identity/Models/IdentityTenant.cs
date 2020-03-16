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
using ActiveTenant;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Models
{
	public class IdentityTenant : IdentityTenant<string> { }

	public class IdentityTenant<TKey> : ITenant<TKey>
	{
		[PersonalData] public virtual TKey Id { get; set; }

		[ProtectedPersonalData] public virtual string Name { get; set; }

		[ProtectedPersonalData] public virtual string NormalizedName { get; set; }

		public virtual string SecurityStamp { get; set; }

		public virtual string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
	}
}