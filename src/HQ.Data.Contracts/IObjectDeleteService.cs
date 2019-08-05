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

namespace HQ.Data.Contracts
{
	public interface IObjectDeleteService
	{
		Task<ObjectDelete> DeleteAsync(Type type, long id);
		Task<ObjectDelete> DeleteAsync(Type type, IObject @object);
		Task DeleteAsync(Type type, SegmentOptions segment);
		Task DeleteAsync(Type type, IEnumerable<IObject> objects, long startingAt = 0, int? count = null);
	}

	public interface IObjectDeleteService<in TObject> where TObject : IObject
	{
		Task<ObjectDelete> DeleteAsync(long id);
		Task<ObjectDelete> DeleteAsync(TObject @object);
		Task DeleteAsync(SegmentOptions segment);
		Task DeleteAsync(IEnumerable<TObject> objects, long startingAt = 0, int? count = null);
	}
}