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

using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Extensions.Identity.Models;
using HQ.Lingo.Queries;

namespace HQ.Extensions.Identity.Stores.Sql
{
    partial class UserStore<TUser, TKey, TRole> : IUserPhoneNumberStoreExtended<TUser>
    {
        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user?.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public async Task<TUser> FindByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SupportsSuperUser && phoneNumber?.ToUpperInvariant() ==
                _security?.Value.SuperUser?.PhoneNumber?.ToUpperInvariant())
                return CreateSuperUserInstance();

            var query = SqlBuilder.Select<TUser>(new {PhoneNumber = phoneNumber, TenantId = _tenantId });
            _connection.SetTypeInfo(typeof(TUser));

            var user = await _connection.Current.QuerySingleOrDefaultAsync<TUser>(query.Sql, query.Parameters);
            return user;
        }
    }
}
