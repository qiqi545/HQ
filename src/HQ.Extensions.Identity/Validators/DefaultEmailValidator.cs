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
using System.Threading.Tasks;
using HQ.Extensions.Identity.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Identity.Validators
{
    /// <summary>
    ///     A default validator that contains equivalent validation logic as ASP.NET Core Identity, with the exception of
    ///     optionally allowing registration without an email address. Any attempts to change the email address after
    ///     the fact should still validate using the default logic.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class DefaultEmailValidator<TUser> : IEmailValidator<TUser> where TUser : class
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly EmailAddressAttribute EmailAddressAttribute = new EmailAddressAttribute();

        private readonly IdentityErrorDescriber _describer;
        private readonly IOptions<IdentityOptionsExtended> _options;

        public DefaultEmailValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
        {
            _describer = describer;
            _options = options;
        }

        public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
        {
            var email = await manager.GetEmailAsync(user);

            if (!_options.Value.User.RequireEmail && string.IsNullOrWhiteSpace(email))
                return;

            if (string.IsNullOrWhiteSpace(email) || !EmailAddressAttribute.IsValid(email))
            {
                errors.Add(_describer.InvalidEmail(email));
                return;
            }

            var exists = await manager.FindByEmailAsync(email);
            if (exists == null)
                return;

            if (manager.Options.User.RequireUniqueEmail && !string.Equals(await manager.GetUserIdAsync(exists),
                    await manager.GetUserIdAsync(user)))
                errors.Add(_describer.DuplicateEmail(email));
        }
    }
}
