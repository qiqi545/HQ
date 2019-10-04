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

namespace HQ.Common
{
	public struct SelfEnumerator<T>
	{
		private readonly List<T> _inner;
		private int _index;

		public SelfEnumerator(List<T> inner)
		{
			_inner = inner;
			_index = 0;
		}

		public T Current => _inner == null || _index == 0 ? default : _inner[_index - 1];

		public bool MoveNext()
		{
			_index++;
			return _inner != null && _inner.Count >= _index;
		}
	}
}