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
using System.Threading.Tasks;
using HQ.Platform.Identity.Extensions;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Models
{
    public class ApplicationValidator<TApplication, TUser, TKey> : IApplicationValidator<TApplication, TUser, TKey>
        where TApplication : IdentityApplication<TKey>
        where TUser : IdentityUserExtended<TKey>
        where TKey : IEquatable<TKey>
    {
        public ApplicationValidator(IdentityErrorDescriber errors = null)
        {
            Describer = errors ?? new IdentityErrorDescriber();
        }

        public IdentityErrorDescriber Describer { get; }

        public virtual async Task<IdentityResult> ValidateAsync(ApplicationManager<TApplication, TUser, TKey> manager,
            TApplication application)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var errors = new List<IdentityError>();
            await ValidateApplicationName(manager, application, errors);
            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateApplicationName(ApplicationManager<TApplication, TUser, TKey> manager,
            TApplication application,
            ICollection<IdentityError> errors)
        {
            var applicationName = await manager.GetApplicationNameAsync(application);
            if (string.IsNullOrWhiteSpace(applicationName))
            {
                errors.Add(Describer.InvalidApplicationName(applicationName));
            }
            else
            {
                if (!string.IsNullOrEmpty(manager.Options.Application.AllowedApplicationNameCharacters) &&
                    applicationName.Any(c => !manager.Options.Application.AllowedApplicationNameCharacters.Contains(c)))
                {
                    errors.Add(Describer.InvalidApplicationName(applicationName));
                }
                else
                {
                    var byNameAsync = await manager.FindByNameAsync(applicationName);
                    var exists = byNameAsync != null;
                    if (exists)
                    {
                        var id = await manager.GetApplicationIdAsync(byNameAsync);
                        exists = !string.Equals(id, await manager.GetApplicationIdAsync(application));
                    }

                    if (!exists)
                    {
                        return;
                    }

                    errors.Add(Describer.DuplicateApplicationName(applicationName));
                }
            }
        }
    }
}
