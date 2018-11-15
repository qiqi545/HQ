using System;
using System.Collections.Generic;

namespace HQ.Common.Extensions
{
    internal struct FuncEnumerable<T, TResult>
    {
        private readonly List<T> _inner;
        private readonly Func<T, TResult> _func;

        public FuncEnumerable(List<T> inner, Func<T, TResult> func)
        {
            _inner = inner;
            _func = func;
        }

        public FuncEnumerator<T, TResult> GetEnumerator()
        {
            return new FuncEnumerator<T, TResult>(_inner, _func);
        }
    }
}
