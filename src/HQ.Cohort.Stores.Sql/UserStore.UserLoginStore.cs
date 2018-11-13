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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Lingo.Queries;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort.Stores.Sql
{
    partial class UserStore<TUser, TKey, TRole>
    {
        public struct AspNetUserLogins
        {
            public TKey UserId { get; set; }
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string ProviderDisplayName { get; set; }
        }
    }

    partial class UserStore<TUser, TKey, TRole> : IUserLoginStore<TUser>
    {
        public async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql =
                "INSERT INTO AspNetUserLogins (LoginProvider, ProviderKey, ProviderDisplayName, UserId) " +
                "VALUES (@LoginProvider, @ProviderKey, @ProviderDisplayName, @UserId)";

            var inserted = await _connection.Current.ExecuteAsync(sql, new
            {
                login.LoginProvider,
                login.ProviderKey,
                login.ProviderDisplayName,
                UserId = user.Id
            });

            Debug.Assert(inserted == 1);
        }

        public async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string sql = "DELETE FROM AspNetUserLogins " +
                               "WHERE UserId = @UserId " +
                               "AND LoginProvider = @LoginProvider " +
                               "AND ProviderKey = @ProviderKey";

            var deleted = await _connection.Current.ExecuteAsync(sql, new
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            });

            Debug.Assert(deleted == 1);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Select<AspNetUserLogins>(new
            {
                UserId = user.Id
            });

            _connection.SetTypeInfo(typeof(AspNetUserLogins));

            var logins = await _connection.Current.QueryAsync<AspNetUserLogins>(query.Sql, query.Parameters);

            return logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName))
                .AsList();
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string getUserId = "SELECT UserId " +
                                     "FROM AspNetUserLogins " +
                                     "WHERE LoginProvider = @LoginProvider " +
                                     "AND ProviderKey = @ProviderKey";

            var userId = await _connection.Current.QuerySingleOrDefaultAsync<string>(getUserId, new
            {
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            });

            if (userId == null) return null;

            const string getUserById = "SELECT * FROM AspNetUsers WHERE Id = @Id";

            return await _connection.Current.QuerySingleAsync<TUser>(getUserById, new {Id = userId});
        }
    }
}
