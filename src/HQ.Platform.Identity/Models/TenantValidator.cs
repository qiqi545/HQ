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
    public class TenantValidator<TTenant, TUser, TKey> : ITenantValidator<TTenant, TUser, TKey>
        where TTenant : IdentityTenant<TKey>
        where TUser : IdentityUserExtended<TKey>
        where TKey : IEquatable<TKey>
    {
        public TenantValidator(IdentityErrorDescriber errors = null)
        {
            Describer = errors ?? new IdentityErrorDescriber();
        }

        public IdentityErrorDescriber Describer { get; }

        public virtual async Task<IdentityResult> ValidateAsync(TenantManager<TTenant, TUser, TKey> manager,
            TTenant tenant)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            var errors = new List<IdentityError>();
            await ValidateTenantName(manager, tenant, errors);
            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateTenantName(TenantManager<TTenant, TUser, TKey> manager, TTenant tenant,
            ICollection<IdentityError> errors)
        {
            var tenantName = await manager.GetTenantNameAsync(tenant);
            if (string.IsNullOrWhiteSpace(tenantName))
            {
                errors.Add(Describer.InvalidTenantName(tenantName));
            }
            else
            {
                if (!string.IsNullOrEmpty(manager.Options.Tenant.AllowedTenantNameCharacters) &&
                    tenantName.Any(c => !manager.Options.Tenant.AllowedTenantNameCharacters.Contains(c)))
                {
                    errors.Add(Describer.InvalidTenantName(tenantName));
                }
                else
                {
                    var byNameAsync = await manager.FindByNameAsync(tenantName);
                    var exists = byNameAsync != null;
                    if (exists)
                    {
                        var id = await manager.GetTenantIdAsync(byNameAsync);
                        exists = !string.Equals(id, await manager.GetTenantIdAsync(tenant));
                    }

                    if (!exists)
                    {
                        return;
                    }

                    errors.Add(Describer.DuplicateTenantName(tenantName));
                }
            }
        }
    }
}
