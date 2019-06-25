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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Validators
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

            if (!_options.Value.User.RequirePhoneNumber && string.IsNullOrWhiteSpace(phoneNumber))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber) || ContainsDeniedPhoneNumberCharacters(phoneNumber) ||
                !PhoneAttribute.IsValid(phoneNumber))
            {
                errors.Add(_describer.InvalidPhoneNumber(phoneNumber));
                return;
            }

            var exists = await manager.FindByNameAsync(phoneNumber);
            if (exists == null)
            {
                return;
            }

            if (!_options.Value.User.RequireUniquePhoneNumber)
            {
                return;
            }

            if (!string.Equals(await manager.GetUserIdAsync(exists), await manager.GetUserIdAsync(user)))
            {
                errors.Add(_describer.DuplicatePhoneNumber(phoneNumber));
            }
        }

        private bool ContainsDeniedPhoneNumberCharacters(string userName)
        {
            return !string.IsNullOrEmpty(_options.Value.User.AllowedPhoneNumberCharacters) &&
                   userName.Any(x => !_options.Value.User.AllowedPhoneNumberCharacters.Contains(x));
        }
    }
}
