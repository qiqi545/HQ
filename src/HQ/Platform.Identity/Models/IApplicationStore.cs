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
    public interface IApplicationStore<TApplication> where TApplication : class
    {
        CancellationToken CancellationToken { get; }

        Task<string> GetApplicationIdAsync(TApplication application, CancellationToken cancellationToken);
        Task<string> GetApplicationNameAsync(TApplication application, CancellationToken cancellationToken);
        Task<int> GetCountAsync(CancellationToken cancellationToken);

        Task<IdentityResult> CreateAsync(TApplication application, CancellationToken cancellationToken);
        Task<IdentityResult> UpdateAsync(TApplication application, CancellationToken cancellationToken);
        Task<IdentityResult> DeleteAsync(TApplication application, CancellationToken cancellationToken);

        Task SetApplicationNameAsync(TApplication application, string name, CancellationToken cancellationToken);
        Task SetNormalizedApplicationNameAsync(TApplication application, string normalizedName, CancellationToken cancellationToken);
        Task<TApplication> FindByIdAsync(string applicationId, CancellationToken cancellationToken);

        Task<IEnumerable<TApplication>> FindByIdsAsync(IEnumerable<string> applicationIds, CancellationToken cancellationToken);
        Task<TApplication> FindByNameAsync(string normalizedApplicationName, CancellationToken cancellationToken);
    }
}
