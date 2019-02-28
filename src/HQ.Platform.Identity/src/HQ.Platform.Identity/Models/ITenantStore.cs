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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Models
{
    public interface ITenantStore<TTenant> where TTenant : class
    {
        CancellationToken CancellationToken { get; }

        Task<string> GetTenantIdAsync(TTenant tenant, CancellationToken cancellationToken);
        Task<string> GetTenantNameAsync(TTenant tenant, CancellationToken cancellationToken);

        Task<IdentityResult> CreateAsync(TTenant tenant, CancellationToken cancellationToken);
        Task<IdentityResult> UpdateAsync(TTenant tenant, CancellationToken cancellationToken);
        Task<IdentityResult> DeleteAsync(TTenant tenant, CancellationToken cancellationToken);

        Task SetTenantNameAsync(TTenant tenant, string name, CancellationToken cancellationToken);
        Task SetNormalizedTenantNameAsync(TTenant tenant, string normalizedName, CancellationToken cancellationToken);

        Task<TTenant> FindByIdAsync(string tenantId, CancellationToken cancellationToken);
        Task<IEnumerable<TTenant>> FindByIdsAsync(IEnumerable<string> tenantIds, CancellationToken cancellationToken);
        Task<TTenant> FindByNameAsync(string normalizedTenantName, CancellationToken cancellationToken);
    }
}
