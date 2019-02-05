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
using System.Linq;
using System.Threading.Tasks;
using HQ.Extensions.Identity.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Identity.Validators
{
    /// <summary>
    ///     A default validator that contains equivalent validation logic as ASP.NET Core Identity, with the exception of
    ///     optionally allowing registration without a username. Any attempts to change the username after
    ///     the fact should still validate using the default logic.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public class DefaultUsernameValidator<TUser> : IUsernameValidator<TUser> where TUser : class
    {
        private readonly IdentityErrorDescriber _describer;
        private readonly IOptions<IdentityOptionsExtended> _options;

        public DefaultUsernameValidator(IdentityErrorDescriber describer, IOptions<IdentityOptionsExtended> options)
        {
            _describer = describer;
            _options = options;
        }

        public async Task ValidateAsync(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
        {
            var username = await manager.GetUserNameAsync(user);

            if (!_options.Value.User.RequireUsername && string.IsNullOrWhiteSpace(username))
                return;

            if (string.IsNullOrWhiteSpace(username) || ContainsDeniedUserNameCharacters(manager, username))
            {
                errors.Add(_describer.InvalidUserName(username));
                return;
            }

            var exists = await manager.FindByNameAsync(username);
            if (exists == null)
                return;

            if (!_options.Value.User.RequireUniqueUsername)
                return;

            if (!string.Equals(await manager.GetUserIdAsync(exists), await manager.GetUserIdAsync(user)))
                errors.Add(_describer.DuplicateUserName(username));
        }

        private static bool ContainsDeniedUserNameCharacters(UserManager<TUser> manager, string userName)
        {
            return !string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                   userName.Any(x => !manager.Options.User.AllowedUserNameCharacters.Contains(x));
        }
    }
}
