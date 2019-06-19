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

namespace HQ.Data.Contracts.Queryable
{
    public interface IQueryableProvider<T>
    {
        /// <summary>
        ///     Determines whether to throw a <see cref="NotSupportedException" /> if <see cref="Queryable" /> is accessed.
        ///     If this is <code>false</code>, then developers may potentially access a <see cref="Queryable" /> that is
        ///     unpredictable, or, if none is available, may materialize the entire underlying collection in order to
        ///     perform a query.
        /// </summary>
        bool IsSafe { get; }

        bool SupportsUnsafe { get; }
        IQueryable<T> Queryable { get; }
        IQueryable<T> UnsafeQueryable { get; }
        ISafeQueryable<T> SafeQueryable { get; }
        IEnumerable<T> SafeAll { get; }
    }
}
