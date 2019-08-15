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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace HQ.Integration.DocumentDb
{
    public interface IDocumentDbRepository<T> where T : IDocument
    {
        
        Task<long> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
		Task<IEnumerable<T>> RetrieveAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);

		Task<T> RetrieveAsync(string id, CancellationToken cancellationToken = default);
		Task<IEnumerable<T>> RetrieveAsync(Func<IQueryable<T>, IQueryable<T>> projection, CancellationToken cancellationToken = default);
		Task<T> RetrieveSingleAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
		Task<T> RetrieveSingleOrDefaultAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
		Task<T> RetrieveFirstAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
		Task<T> RetrieveFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);

		Task<Document> CreateAsync(T item, CancellationToken cancellationToken = default);
		Task<Document> UpdateAsync(string id, T item, CancellationToken cancellationToken = default);
		Task<Document> UpsertAsync(T item, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
