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
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Data.Contracts;
using HQ.Platform.Security.AspNetCore.Mvc.Models;

namespace HQ.Platform.Identity.Models
{
	public interface IUserService
	{
		Task<Operation<int>> GetCountAsync();
	}

	public interface IUserService<TUser> : IUserService
	{
		IQueryable<TUser> Users { get; }

		Task<Operation<IEnumerable<TUser>>> GetAsync();
		Task<Operation<TUser>> CreateAsync(CreateUserModel model);
		Task<Operation> DeleteAsync(string id);

		Task<Operation<TUser>> FindByIdAsync(string id);
		Task<Operation<TUser>> FindByEmailAsync(string email);
		Task<Operation<TUser>> FindByNameAsync(string username);
		Task<Operation<TUser>> FindByPhoneNumberAsync(string phoneNumber);
		Task<Operation<TUser>> FindByLoginAsync(string loginProvider, string providerKey);
		Task<Operation<TUser>> FindByAsync(Expression<Func<TUser, bool>> predicate);
		Task<Operation<TUser>> FindByIdentity(IdentityType identityType, string identity);

		Task<Operation<IList<string>>> GetRolesAsync(TUser user);
		Task<Operation> AddToRoleAsync(TUser user, string role);
		Task<Operation> RemoveFromRoleAsync(TUser user, string role);

		Task<Operation<IList<Claim>>> GetClaimsAsync(TUser user);
		Task<Operation> AddClaimAsync(TUser user, Claim claim);
		Task<Operation> RemoveClaimAsync(TUser user, Claim claim);
		Task<Operation> AddClaimsAsync(TUser user, IEnumerable<Claim> claims);
		Task<Operation> RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims);

		Task<Operation<string>> GenerateChangePhoneNumberTokenAsync(TUser user, string phoneNumber);
		Task<Operation<string>> GenerateChangeEmailTokenAsync(TUser user, string newEmail);
		Task<Operation<string>> GenerateEmailConfirmationTokenAsync(TUser user);
		Task<Operation<string>> GeneratePasswordResetTokenAsync(TUser user);
		Task<Operation<IEnumerable<string>>> GenerateNewTwoFactorRecoveryCodesAsync(TUser user, int number);

		Task<Operation> ChangePhoneNumberAsync(TUser user, string phoneNumber, string token);
		Task<Operation> ChangeEmailAsync(TUser user, string newEmail, string token);
		Task<Operation> ChangePasswordAsync(TUser user, string token, string newPassword);
		Task<Operation> ConfirmEmailAsync(TUser user, string token);
		Task<Operation> ResetPasswordAsync(TUser user, string token, string newPassword);

		Task<Operation> UpdateAsync(TUser user);

		Task<Operation<TUser>> LinkExternalIdentityAsync(ClaimsPrincipal principal, string loginProvider,
			string providerKeyClaimType = null, string displayName = null);
	}
}