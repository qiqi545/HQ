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
using System.Threading.Tasks;
using HQ.Data.Contracts;

namespace HQ.Platform.Identity.Models
{
    public interface IApplicationService<TApplication>
    {
        Task<Operation<IEnumerable<TApplication>>> GetAsync();
        Task<Operation<TApplication>> CreateAsync(CreateApplicationModel model);
        Task<Operation> UpdateAsync(TApplication application);
        Task<Operation> DeleteAsync(string id);

        Task<Operation<TApplication>> FindByIdAsync(string id);
        Task<Operation<TApplication>> FindByNameAsync(string name);

        Task<Operation<IEnumerable<TApplication>>> FindByPhoneNumberAsync(string phoneNumber);
        Task<Operation<IEnumerable<TApplication>>> FindByEmailAsync(string email);
        Task<Operation<IEnumerable<TApplication>>> FindByUserNameAsync(string username);
    }
}
