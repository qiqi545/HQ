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
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Queryable;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;

namespace HQ.Platform.Identity.Services
{
    public class ApplicationService<TApplication, TUser, TKey> : IApplicationService<TApplication>
        where TApplication : IdentityApplication<TKey>
        where TUser : IdentityUserExtended<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IQueryableProvider<TApplication> _queryableProvider;
        private readonly ApplicationManager<TApplication, TUser, TKey> _applicationManager;

        public ApplicationService(ApplicationManager<TApplication, TUser, TKey> applicationManager,
            IQueryableProvider<TApplication> queryableProvider)
        {
            _applicationManager = applicationManager;
            _queryableProvider = queryableProvider;
        }

        public IQueryable<TApplication> Applications => _applicationManager.Applications;

        public async Task<Operation<int>> GetCountAsync()
        {
            var result = await _applicationManager.GetCountAsync();
            var operation = new Operation<int>(result);
            return operation;
        }

        public Task<Operation<IEnumerable<TApplication>>> GetAsync()
        {
            var all = _queryableProvider.SafeAll ?? Applications;
            return Task.FromResult(new Operation<IEnumerable<TApplication>>(all));
        }

        public async Task<Operation<TApplication>> CreateAsync(CreateApplicationModel model)
        {
            var application = (TApplication)FormatterServices.GetUninitializedObject(typeof(TApplication));
            application.Name = model.Name;
            application.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            var result = await _applicationManager.CreateAsync(application);
            return result.ToOperation(application);
        }

        public async Task<Operation> UpdateAsync(TApplication application)
        {
            var result = await _applicationManager.UpdateAsync(application);
            return result.ToOperation();
        }

        public async Task<Operation> DeleteAsync(string id)
        {
            var operation = await FindByIdAsync(id);
            if (!operation.Succeeded)
            {
                return operation;
            }

            var deleted = await _applicationManager.DeleteAsync(operation.Data);
            return deleted.ToOperation();
        }

        

        public async Task<Operation<TApplication>> FindByIdAsync(string id)
        {
            var application = await _applicationManager.FindByIdAsync(id);
            return application == null
                ? new Operation<TApplication>(new Error(ErrorEvents.ResourceMissing, ErrorStrings.ApplicationNotFound,
                    HttpStatusCode.NotFound))
                : new Operation<TApplication>(application);
        }

        public async Task<Operation<TApplication>> FindByNameAsync(string name)
        {
            return new Operation<TApplication>(await _applicationManager.FindByNameAsync(name));
        }

        public async Task<Operation<IEnumerable<TApplication>>> FindByPhoneNumberAsync(string phoneNumber)
        {
            return new Operation<IEnumerable<TApplication>>(await _applicationManager.FindByPhoneNumberAsync(phoneNumber));
        }

        public async Task<Operation<IEnumerable<TApplication>>> FindByEmailAsync(string email)
        {
            return new Operation<IEnumerable<TApplication>>(await _applicationManager.FindByEmailAsync(email));
        }

        public async Task<Operation<IEnumerable<TApplication>>> FindByUserNameAsync(string username)
        {
            return new Operation<IEnumerable<TApplication>>(await _applicationManager.FindByUserNameAsync(username));
        }
    }
}
