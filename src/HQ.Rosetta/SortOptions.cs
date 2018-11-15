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
using HQ.Common.Extensions;
using HQ.Rosetta.Configuration;

namespace HQ.Rosetta
{
    public class SortOptions : IQueryValidator, IEnumerable<KeyValuePair<string, bool>>
    {
        public static readonly SortOptions Empty = new SortOptions();

        public List<Sort> Fields { get; } = new List<Sort>();

        public IEnumerator<KeyValuePair<string, bool>> GetEnumerator()
        {
            foreach (var field in Fields)
                yield return new KeyValuePair<string, bool>(field.Field, field.Descending);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Validate(Type type, QueryOptions options, out IEnumerable<Error> errors)
        {
            var list = FieldValidations.MustExistOnType(Fields.Enumerate(x => x.Field));
            errors = list;
            return list.Count == 0;
        }
    }
}
