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
using System.Threading.Tasks;
using ActiveErrors;

namespace HQ.Data.Contracts
{
	public interface IObjectDeleteRepository<in TKey> where TKey : IEquatable<TKey>
	{
		Task<Operation<ObjectDelete>> DeleteAsync(Type type, TKey id);
		Task<Operation<ObjectDelete>> DeleteAsync(Type type, IObject @object);
		Task<Operation<IEnumerable<ObjectDelete>>> DeleteAsync(Type type, SegmentOptions segment);

		Task<Operation<IEnumerable<ObjectDelete>>> DeleteAsync(Type type, IEnumerable<IObject> objects,
			long startingAt = 0, int? count = null);
	}

	public interface IObjectDeleteRepository<in TObject, in TKey> where TObject : IObject<TKey> where TKey : IEquatable<TKey>
	{
		Task<Operation<ObjectDelete>> DeleteAsync(TKey id);
		Task<Operation<ObjectDelete>> DeleteAsync(TObject @object);
		Task<Operation> DeleteAsync(SegmentOptions segment);
		Task<Operation> DeleteAsync(IEnumerable<TObject> objects, long startingAt = 0, int? count = null);
	}
}