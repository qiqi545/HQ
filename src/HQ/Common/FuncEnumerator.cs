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

namespace HQ.Common
{
    public struct FuncEnumerator<T, TResult>
    {
        private readonly List<T> _inner;
        private readonly Func<T, TResult> _func;
        private int _index;

        public FuncEnumerator(List<T> inner, Func<T, TResult> func)
        {
            _inner = inner;
            _func = func;
            _index = 0;
        }

        public TResult Current => _inner == null || _index == 0 ? default : _func(_inner[_index - 1]);

        public bool MoveNext()
        {
            _index++;
            return _inner != null && _inner.Count >= _index;
        }
    }
}
