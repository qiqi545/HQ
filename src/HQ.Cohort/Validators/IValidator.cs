// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Validators
{
	public interface IValidator<TUser> where TUser : class
	{
		Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors);
	}
}