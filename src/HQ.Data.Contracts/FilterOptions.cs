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
using System.Linq.Expressions;
using ActiveErrors;
using HQ.Common;
using HQ.Data.Contracts.Configuration;
using TypeKitchen;

namespace HQ.Data.Contracts
{
	public class FilterOptions : IQueryValidator
	{
		public static readonly FilterOptions Empty = new FilterOptions();

		public List<Filter> Fields { get; set; } = new List<Filter>();

		public bool Validate(Type type, QueryOptions options, out IList<Error> errors)
		{
			var list = FieldValidations.MustExistOnType(Fields.Enumerate(x => x.Field));
			errors = list;
			return list.Count == 0;
		}

		private static FilterOperator BuildOperator(string member)
		{
			return member == "EqualTo" ? FilterOperator.Equal : FilterOperator.NotEqual;
		}

		public static FilterOptions FromType<T>(params Expression<Func<T, object>>[] filterBy)
		{
			var options = new FilterOptions();

			var clauses = filterBy.Length;

#if !CSHARP_7_2
			for (var i = 0; i < filterBy.Length; i++)
				if (filterBy[i] == null)
					clauses--;
#endif

			for (var i = 0; i < filterBy.Length; i++)
			{
				var entry = filterBy[i];

#if !CSHARP_7_2
				if (entry == null)
					continue;
#endif

				// explicit case: user typed Field.EqualTo() or Field.NotEqualTo
				if (entry.Body is MethodCallExpression call && call.Arguments.Count == 1 &&
				    call.Arguments[0] is MemberExpression callMember)
					options.Fields.Add(new Filter
					{
						Field = callMember.Member.Name, Operator = BuildOperator(call.Method.Name)
					});
				// implicit case: user typed Field and omitted EqualTo() or NotEqualTo()
				else if (entry.Body is MemberExpression member)
					options.Fields.Add(new Filter {Field = member.Member.Name});
				// special case: user performed the explicit case, but we have multiple params in C# 7.2, which thunks with conversion operations
				else if (entry.Body is MethodCallExpression callConvert &&
				         callConvert.Arguments.Count == 1 &&
				         callConvert.Arguments[0] is UnaryExpression unary &&
				         unary.Operand is MemberExpression convertMember)
					options.Fields.Add(new Filter
					{
						Field = convertMember.Member.Name, Operator = BuildOperator(convertMember.Member.Name)
					});
				// special case: user performed the implicit case, but we have multiple params in C# 7.2, which thunks with conversion operations
				else if (entry.Body is UnaryExpression callConvertUnary &&
				         callConvertUnary.Operand is MemberExpression convertMemberOmitted)
					options.Fields.Add(new Filter {Field = convertMemberOmitted.Member.Name});
				else
					throw new NotSupportedException("Ordering by an expression has a strict set of outcomes");
			}

			return options;
		}
	}
}