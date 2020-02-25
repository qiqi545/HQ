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
using System.Linq.Expressions;
using System.Threading.Tasks;
using HQ.Data.Contracts.Queryable;
using Metrics;
using Microsoft.Azure.Documents.Client;

namespace HQ.Integration.DocumentDb.SessionManagement
{
	internal class DocumentDbSafeQueryable<TUser> : DocumentDbQueries, ISafeQueryable<TUser>
	{
		public DocumentDbSafeQueryable(DocumentClient client, Uri collectionUri, IMetricsHost<DocumentClient> metrics) :
			base(client, collectionUri, metrics)
		{
		}

		public async Task<TUser> SingleOrDefaultAsync(Expression<Func<TUser, bool>> predicate)
		{
			return await SingleOrDefaultAsync<TUser>(predicate);
		}

		public TUser SingleOrDefault(Expression<Func<TUser, bool>> predicate)
		{
			return SingleOrDefault<TUser>(predicate);
		}

		public async Task<TUser> FirstOrDefaultAsync(Expression<Func<TUser, bool>> predicate)
		{
			return (await FindByPredicateAsync(predicate)).FirstOrDefault();
		}

		public TUser FirstOrDefault(Expression<Func<TUser, bool>> predicate)
		{
			return FindByPredicate(predicate).FirstOrDefault();
		}

		public IEnumerable<TUser> Where(Expression<Func<TUser, bool>> predicate)
		{
			return FindByPredicate(predicate);
		}

		public async Task<IEnumerable<TUser>> WhereAsync(Expression<Func<TUser, bool>> predicate)
		{
			return await FindByPredicateAsync(predicate);
		}
	}
}