using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ActiveAuth.Models;
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