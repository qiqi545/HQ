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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HQ.Common;

namespace HQ.Extensions.CodeGeneration.Helpers
{
    public sealed class StringBufferPool : IDisposable, IEnumerable<KeyValuePair<string, IStringBuffer>>
    {
        private readonly Dictionary<string, IStringBuffer> _inner;

        public StringBufferPool()
        {
            _inner = new Dictionary<string, IStringBuffer>();
        }

        public void Dispose()
        {
            foreach (var inner in _inner.Values)
                inner.Dispose();
        }

        IEnumerator<KeyValuePair<string, IStringBuffer>> IEnumerable<KeyValuePair<string, IStringBuffer>>.
            GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        public IStringBuffer GetOrAdd(string key)
        {
            if (!_inner.TryGetValue(key, out var buffer))
                _inner.Add(key, buffer = new StringBuffer());
            return buffer;
        }

        private class StringBuffer : IStringBuffer
        {
            private readonly StringBuilder _inner;

            public StringBuffer()
            {
                _inner = StringBuilderPool.Pool.Get();
            }

            public void AppendLine(string value)
            {
                _inner.AppendLine(value);
            }

            public void AppendLine()
            {
                _inner.AppendLine();
            }

            public void Append(string value)
            {
                _inner.Append(value);
            }

            public void Append(object value)
            {
                _inner.Append(value);
            }

            public void Dispose()
            {
                StringBuilderPool.Pool.Return(_inner);
            }

            public override string ToString()
            {
                return _inner.ToString();
            }
        }
    }
}
