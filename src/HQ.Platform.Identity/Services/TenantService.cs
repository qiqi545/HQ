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
using ActiveErrors;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Queryable;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;

namespace HQ.Platform.Identity.Services
{
	public class TenantService<TTenant, TUser, TKey> : ITenantService<TTenant>
		where TTenant : IdentityTenant<TKey>
		where TUser : IdentityUserExtended<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IQueryableProvider<TTenant> _queryableProvider;
		private readonly TenantManager<TTenant, TUser, TKey> _tenantManager;

		public TenantService(TenantManager<TTenant, TUser, TKey> tenantManager,
			IQueryableProvider<TTenant> queryableProvider)
		{
			_tenantManager = tenantManager;
			_queryableProvider = queryableProvider;
		}

		public IQueryable<TTenant> Tenants => _tenantManager.Tenants;

		public async Task<Operation<int>> GetCountAsync()
		{
			var result = await _tenantManager.GetCountAsync();
			var operation = new Operation<int>(result);
			return operation;
		}

		public Task<Operation<IEnumerable<TTenant>>> GetAsync()
		{
			var all = _queryableProvider.SafeAll ?? Tenants;
			return Task.FromResult(new Operation<IEnumerable<TTenant>>(all));
		}

		public async Task<Operation<TTenant>> CreateAsync(CreateTenantModel model)
		{
			var tenant = (TTenant) FormatterServices.GetUninitializedObject(typeof(TTenant));
			tenant.Name = model.Name;
			tenant.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

			var result = await _tenantManager.CreateAsync(tenant);
			return result.ToOperation(tenant);
		}

		public async Task<Operation> UpdateAsync(TTenant tenant)
		{
			var result = await _tenantManager.UpdateAsync(tenant);
			return result.ToOperation();
		}

		public async Task<Operation> DeleteAsync(string id)
		{
			var operation = await FindByIdAsync(id);
			if (!operation.Succeeded)
			{
				return operation;
			}

			var deleted = await _tenantManager.DeleteAsync(operation.Data);
			return deleted.ToOperation();
		}

		public async Task<Operation<TTenant>> FindByIdAsync(string id)
		{
			var tenant = await _tenantManager.FindByIdAsync(id);
			return tenant == null
				? new Operation<TTenant>(new Error(ErrorEvents.ResourceMissing, ErrorStrings.TenantNotFound,
					HttpStatusCode.NotFound))
				: new Operation<TTenant>(tenant);
		}

		public async Task<Operation<TTenant>> FindByNameAsync(string name)
		{
			return new Operation<TTenant>(await _tenantManager.FindByNameAsync(name));
		}

		public async Task<Operation<IEnumerable<TTenant>>> FindByPhoneNumberAsync(string phoneNumber)
		{
			return new Operation<IEnumerable<TTenant>>(await _tenantManager.FindByPhoneNumberAsync(phoneNumber));
		}

		public async Task<Operation<IEnumerable<TTenant>>> FindByEmailAsync(string email)
		{
			return new Operation<IEnumerable<TTenant>>(await _tenantManager.FindByEmailAsync(email));
		}

		public async Task<Operation<IEnumerable<TTenant>>> FindByUserNameAsync(string username)
		{
			return new Operation<IEnumerable<TTenant>>(await _tenantManager.FindByUserNameAsync(username));
		}
	}
}