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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Data.Contracts.Queryable;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Extensions;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class UserStore<TUser, TKey, TRole> : IQueryableUserStore<TUser>
        where TUser : IdentityUserExtended<TKey>
        where TKey : IEquatable<TKey>
        where TRole : IdentityRole<TKey>
    {
        private const string SuperUserDefaultUserName = "superuser";
        private const string SuperUserDefaultEmail = "superuser@email.com";
        private const string SuperUserDefaultPhoneNumber = "9999999999";
        private const string SuperUserGuidId = "87BA0A16-7253-4A6F-A8D4-82DFA1F723C1";
        private const string SuperUserSecurityStamp = "A2ECC018-9B97-420B-815E-9D5B595BFA86";
        private const int SuperUserNumberId = int.MaxValue;

        // ReSharper disable once StaticMemberInGenericType
        private readonly IDataConnection _connection;
        private readonly IOptions<IdentityOptionsExtended> _identity;
        private readonly IPasswordHasher<TUser> _passwordHasher;
        private readonly IQueryableProvider<TUser> _queryable;
        private readonly RoleManager<TRole> _roles;
        private readonly IOptions<SecurityOptions> _security;

        private readonly int _tenantId;
        private readonly string _tenantName;

        public UserStore(IDataConnection connection,
            IPasswordHasher<TUser> passwordHasher,
            RoleManager<TRole> roles,
            IQueryableProvider<TUser> queryable,
            IOptions<IdentityOptionsExtended> identity,
            IOptions<SecurityOptions> security,
            IServiceProvider serviceProvider)
        {
            serviceProvider.TryGetRequestAbortCancellationToken(out var cancellationToken);
            serviceProvider.TryGetTenantId(out _tenantId);
            serviceProvider.TryGetTenantName(out _tenantName);

            CancellationToken = cancellationToken;

            _connection = connection;
            _passwordHasher = passwordHasher;
            _roles = roles;
            _queryable = queryable;

            _identity = identity;
            _security = security;
        }

        public IQueryable<TUser> Users => MaybeQueryable();

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.TenantId = _tenantId;
            user.ConcurrencyStamp = user.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            if (user.Id == null)
            {
                var idType = typeof(TKey);
                var id = Guid.NewGuid();
                if (idType == typeof(Guid))
                {
                    user.Id = (TKey) (object) id;
                }
                else if (idType == typeof(string))
                {
                    user.Id = (TKey) (object) $"{id}";
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            var query = SqlBuilder.Insert(user);
            _connection.SetTypeInfo(typeof(TUser));
            var inserted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

            Debug.Assert(inserted == 1);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = SqlBuilder.Delete<TUser>(new {user.Id, TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(TUser));
            var deleted = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);

            Debug.Assert(deleted == 1);
            return IdentityResult.Success;
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SupportsSuperUser && userId == SuperUserGuidId)
            {
                return CreateSuperUserInstance();
            }

            var query = SqlBuilder.Select<TUser>(new {Id = userId, TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(TUser));

            var user = await _connection.Current.QuerySingleOrDefaultAsync<TUser>(query.Sql, query.Parameters);
            return user;
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SupportsSuperUser && normalizedUserName?.ToUpperInvariant() ==
                _security?.Value.SuperUser?.Username?.ToUpperInvariant())
            {
                return CreateSuperUserInstance();
            }

            var query = SqlBuilder.Select<TUser>(new {NormalizedUserName = normalizedUserName, TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(TUser));

            var user = await _connection.Current.QuerySingleOrDefaultAsync<TUser>(query.Sql, query.Parameters);
            return user;
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user?.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"{user.Id}");
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user?.UserName);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            user.ConcurrencyStamp = user.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            var query = SqlBuilder.Update(user, new {user.Id, TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(TUser));

            var updated = await _connection.Current.ExecuteAsync(query.Sql, query.Parameters);
            Debug.Assert(updated == 1);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public CancellationToken CancellationToken { get; }

        public bool SupportsSuperUser => _security?.Value?.SuperUser?.Enabled ?? false;

        public async Task<IEnumerable<TUser>> FindAllByNameAsync(string normalizedUserName,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SupportsSuperUser && normalizedUserName?.ToUpperInvariant() ==
                _security?.Value.SuperUser?.Username?.ToUpperInvariant())
            {
                return new[] {CreateSuperUserInstance()};
            }

            var query = SqlBuilder.Select<TUser>(new {NormalizedUserName = normalizedUserName});
            _connection.SetTypeInfo(typeof(TUser));

            var users = await _connection.Current.QueryAsync<TUser>(query.Sql, query.Parameters);
            return users;
        }

        private IQueryable<TUser> MaybeQueryable()
        {
            if (_queryable.IsSafe)
            {
                return _queryable.Queryable;
            }

            if (_queryable.SupportsUnsafe)
            {
                return _queryable.UnsafeQueryable;
            }

            return Task.Run(GetAllUsersAsync, CancellationToken).Result.AsQueryable();
        }

        private Task<IEnumerable<TUser>> GetAllUsersAsync()
        {
            var query = SqlBuilder.Select<TUser>(new {TenantId = _tenantId});
            _connection.SetTypeInfo(typeof(TUser));
            var users = _connection.Current.QueryAsync<TUser>(query.Sql, query.Parameters);
            return users;
        }

        private TUser CreateSuperUserInstance()
        {
            var superuser = Activator.CreateInstance<TUser>();
            if (typeof(TKey) == typeof(Guid))
            {
                superuser.Id = (TKey) (object) Guid.Parse(SuperUserGuidId);
            }
            else if (typeof(TKey) == typeof(string))
            {
                superuser.Id = (TKey) (object) SuperUserGuidId;
            }
            else
            {
                superuser.Id = (TKey) (object) SuperUserNumberId;
            }

            var options = _security?.Value.SuperUser;

            superuser.UserName = options?.Username ?? SuperUserDefaultUserName;
            superuser.NormalizedUserName = superuser.UserName?.ToUpperInvariant();
            superuser.PhoneNumber = options?.PhoneNumber?.ToUpperInvariant() ?? SuperUserDefaultPhoneNumber;
            superuser.PhoneNumberConfirmed = true;
            superuser.Email = options?.Email ?? SuperUserDefaultEmail;
            superuser.NormalizedEmail = options?.Email?.ToUpperInvariant() ?? SuperUserDefaultEmail.ToUpperInvariant();
            superuser.EmailConfirmed = true;
            superuser.LockoutEnabled = false;
            superuser.TwoFactorEnabled = false;
            superuser.SecurityStamp = SuperUserSecurityStamp;
            superuser.ConcurrencyStamp = $"{Guid.NewGuid()}";
            superuser.PasswordHash = _passwordHasher.HashPassword(superuser, options?.Password);

            return superuser;
        }
    }
}
