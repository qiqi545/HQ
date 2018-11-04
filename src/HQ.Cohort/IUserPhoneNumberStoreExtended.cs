// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort
{
	public interface IUserPhoneNumberStoreExtended<TUser> :
		IUserStoreExtended<TUser>,
		IUserPhoneNumberStore<TUser> where TUser : class
	{
		Task<TUser> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);
	}
}