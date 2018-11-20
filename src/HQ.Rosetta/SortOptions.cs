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
using System.Linq.Expressions;
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

        public bool Validate(Type type, QueryOptions options, out IList<Error> errors)
        {
            var list = FieldValidations.MustExistOnType(Fields.Enumerate(x => x.Field));
            errors = list;
            return list.Count == 0;
        }

        public static SortOptions FromType<T>(params Expression<Func<T, object>>[] orderBy)
        {
            var options = new SortOptions();

            var clauses = orderBy.Length;

#if !CSHARP_7_2
            for (var i = 0; i < orderBy.Length; i++)
                if (orderBy[i] == null)
                    clauses--;
#endif

            for (var i = 0; i < orderBy.Length; i++)
            {
                var entry = orderBy[i];

#if !CSHARP_7_2
                if (entry == null)
                    continue;
#endif

                // explicit case: user typed Field.Asc() or Field.Desc()
                if (entry.Body is MethodCallExpression call && call.Arguments.Count == 1 &&
                    call.Arguments[0] is MemberExpression callMember)
                    options.Fields.Add(new Sort
                    {
                        Field = callMember.Member.Name,
                        Descending = call.Method.Name == "Desc"
                    });
                // implicit case: user typed Field and omitted Asc() or Desc()
                else if (entry.Body is MemberExpression member)
                    options.Fields.Add(new Sort
                    {
                        Field = member.Member.Name
                    });
                // special case: user performed the explicit case, but we have multiple params in C# 7.2, which thunks with conversion operations
                else if (entry.Body is MethodCallExpression callConvert &&
                         callConvert.Arguments.Count == 1 &&
                         callConvert.Arguments[0] is UnaryExpression unary &&
                         unary.Operand is MemberExpression convertMember)
                    options.Fields.Add(new Sort
                    {
                        Field = convertMember.Member.Name,
                        Descending = callConvert.Method.Name == "Desc"
                    });
                // special case: user performed the implicit case, but we have multiple params in C# 7.2, which thunks with conversion operations
                else if (entry.Body is UnaryExpression callConvertUnary &&
                         callConvertUnary.Operand is MemberExpression convertMemberOmitted)
                    options.Fields.Add(new Sort
                    {
                        Field = convertMemberOmitted.Member.Name
                    });
                else
                    throw new NotSupportedException("Ordering by an expression has a strict set of outcomes");
            }

            return options;
        }
    }
}
