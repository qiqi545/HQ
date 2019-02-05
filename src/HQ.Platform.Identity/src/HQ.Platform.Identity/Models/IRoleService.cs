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
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Data.Contracts;

namespace HQ.Platform.Identity.Models
{
    public interface IRoleService<TRole>
    {
        IQueryable<TRole> Roles { get; }

        Task<Operation<IEnumerable<TRole>>> GetAsync();
        Task<Operation<TRole>> CreateAsync(CreateRoleModel model);
        Task<Operation> DeleteAsync(string id);

        Task<Operation<TRole>> FindByIdAsync(string id);
        Task<Operation<TRole>> FindByNameAsync(string roleName);

        Task<Operation<IList<Claim>>> GetClaimsAsync(TRole role);
        Task<Operation<IList<Claim>>> GetAllRoleClaimsAsync();
        Task<Operation> AddClaimAsync(TRole role, Claim claim);
        Task<Operation> RemoveClaimAsync(TRole role, Claim claim);
    }
}
