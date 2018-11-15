using System;
using System.Collections.Generic;

namespace HQ.Common.Extensions
{
    internal struct FuncEnumerator<T, TResult>
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

        public void Reset()
        {
            _index = 0;
        }
    }
}
