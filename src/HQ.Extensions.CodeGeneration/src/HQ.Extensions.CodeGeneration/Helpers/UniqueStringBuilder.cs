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
using System.Text;
using HQ.Common;

namespace HQ.Extensions.CodeGeneration.Helpers
{
    public sealed class UniqueStringBuilder : IDisposable
    {
        private readonly HashSet<string> _bucket;
        private readonly StringBuilder _buffer;

        public StringSort? Sort;

        public UniqueStringBuilder() : this(new StringBuilder())
        {
        }

        public UniqueStringBuilder(StringBuilder buffer)
        {
            _buffer = buffer;
            _bucket = new HashSet<string>();
        }

        public override string ToString()
        {
            var output = StringBuilderPool.Scoped(sb =>
            {
                if (Sort.HasValue)
                    switch (Sort)
                    {
                        case StringSort.Ascending:
                            foreach (var line in _bucket.OrderBy(s => s))
                                sb.AppendLine(line);
                            break;
                        case StringSort.Descending:
                            foreach (var line in _bucket.OrderByDescending(s => s))
                                sb.AppendLine(line);
                            break;
                        case null:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                else
                    foreach (var line in _bucket)
                        sb.AppendLine(line);
            });
            _buffer.Append(output);
            return output;
        }

        public void AppendLine(string value)
        {
            _bucket.Add(value);
        }

        public void Dispose()
        {
            _buffer.Clear();
            _bucket?.Clear();
        }
    }
}
