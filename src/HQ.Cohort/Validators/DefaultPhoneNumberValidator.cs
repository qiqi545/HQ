// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HQ.Cohort.Configuration;
using HQ.Cohort.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.Validators
{
	public class DefaultPhoneNumberValidator<TUser> : IPhoneNumberValidator<TUser> where TUser : class
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly PhoneAttribute PhoneAttribute = new PhoneAttribute();

		private readonly IdentityErrorDescriber _describer;
		private readonly IOptions<IdentityOptionsExtended> _options;

		public DefaultPhoneNumberValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
		{
			_describer = describer;
			_options = options;
		}

		public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
		{
			var phoneNumber = await manager.GetPhoneNumberAsync(user);

			if (await CanRegisterWithoutPhoneNumber(manager, user, phoneNumber))
				return;

			if (string.IsNullOrWhiteSpace(phoneNumber) ||
			    ContainsDeniedPhoneNumberCharacters(phoneNumber) ||
			    !PhoneAttribute.IsValid(phoneNumber))
			{
				errors.Add(_describer.InvalidPhoneNumber(phoneNumber));
				return;
			}

			var exists = await manager.FindByNameAsync(phoneNumber);
			if (exists == null)
				return;

			if (!_options.Value.User.RequireUniquePhoneNumber)
				return;

			if (!string.Equals(await manager.GetUserIdAsync(exists), await manager.GetUserIdAsync(user)))
				errors.Add(_describer.DuplicatePhoneNumber(phoneNumber));
		}

		private bool ContainsDeniedPhoneNumberCharacters(string userName)
		{
			return !string.IsNullOrEmpty(_options.Value.User.AllowedPhoneNumberCharacters) &&
			       userName.Any(x => !_options.Value.User.AllowedPhoneNumberCharacters.Contains(x));
		}

		private async Task<bool> CanRegisterWithoutPhoneNumber(UserManager<TUser> manager, TUser user,
			string phoneNumber)
		{
			return !_options.Value.User.RequirePhoneNumberOnRegister &&
			       string.IsNullOrWhiteSpace(phoneNumber) &&
			       await manager.GetUserIdAsync(user) == null;
		}
	}
}