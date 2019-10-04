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
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HQ.Data.Sql.Queries;
using HQ.Platform.Identity.Stores.Sql.Models;
using Microsoft.AspNetCore.Identity;

namespace HQ.Platform.Identity.Stores.Sql
{
	partial class UserStore<TUser, TKey, TRole> : IUserTwoFactorRecoveryCodeStore<TUser>
	{
		public async Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			const string update = "UPDATE AspNetUserTokens " +
			                      "SET Value = @Value " +
			                      "WHERE UserId = @UserId " +
			                      "AND LoginProvider = '[AspNetUserStore]' " +
			                      "AND Name = 'RecoveryCodes' " +
			                      "AND TenantId = @TenantId";

			await _connection.Current.ExecuteAsync(update,
				new {UserId = user.Id, Value = string.Join(";", recoveryCodes), TenantId = _tenantId});
		}

		public async Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			const string sql = "SELECT Value FROM AspNetUserTokens " +
			                   "WHERE UserId = @UserId " +
			                   "AND LoginProvider = '[AspNetUserStore]' " +
			                   "AND Name = 'RecoveryCodes' " +
			                   "AND TenantId = @TenantId";

			var value = await _connection.Current.QuerySingleOrDefaultAsync<string>(sql,
				new {UserId = user.Id, TenantId = _tenantId});

			if (!string.IsNullOrWhiteSpace(value))
			{
				var recoveryCodes = value.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);

				return recoveryCodes.Contains(code);
			}

			return false;
		}

		public async Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var query = SqlBuilder.Select<AspNetUserTokens<TKey>>(new
			{
				UserId = user.Id, Name = "RecoveryCodes", LoginProvider = "[AspNetUserStore]", TenantId = _tenantId
			});
			_connection.SetTypeInfo(typeof(AspNetUserTokens<TKey>));

			var token = await _connection.Current.QuerySingleOrDefaultAsync<AspNetUserTokens<TKey>>(query.Sql,
				query.Parameters);

			if (!string.IsNullOrWhiteSpace(token.Value))
			{
				var recoveryCodes = token.Value.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
				return recoveryCodes.Length;
			}

			return 0;
		}
	}
}