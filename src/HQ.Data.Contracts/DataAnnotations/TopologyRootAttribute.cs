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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using HQ.Common;
using HQ.Data.Contracts.Topology;
using TypeKitchen;

namespace HQ.Data.Contracts.DataAnnotations
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class TopologyRootAttribute : ValidationAttribute
	{
		public TopologyRootAttribute() => ErrorMessage = "{0} has at least one cycle.";

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var accessor = ReadAccessor.Create(value, AccessorMemberTypes.Properties, AccessorMemberScope.Public, out var members);

			var nodes = new HashSet<INode<string>>();
			var visited = new HashSet<object>();
			WalkGraph(visited, value, members, accessor, nodes);

			var cycles = nodes.HasCycles(node => node.Dependents);

			return cycles ? new ValidationResult(string.Format(CultureInfo.CurrentCulture, ErrorMessageString, accessor.Type.Name)) : ValidationResult.Success;
		}

		private static void WalkGraph(ISet<object> visited, object value, AccessorMembers members,
			ITypeReadAccessor accessor, ISet<INode<string>> nodes)
		{
			if (value is INode<string> node)
			{
				nodes.Add(node);
				visited.Add(node);
			}

			foreach (var member in members)
			{
				var memberTypeInfo = member.Type.GetTypeInfo();

				if (memberTypeInfo.ImplementedInterfaces.Contains(typeof(INode<string>)) &&
				    accessor.TryGetValue(value, member.Name, out var element) && element is INode<string> memberNode)
					nodes.Add(memberNode);

				if (!memberTypeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)) ||
				    !accessor.TryGetValue(value, member.Name, out var collection) ||
				    !(collection is IEnumerable enumerable))
					continue;

				if (visited.Contains(enumerable))
					continue;

				visited.Add(enumerable);

				foreach (var item in enumerable)
				{
					var collectionAccessor = ReadAccessor.Create(item, AccessorMemberTypes.Properties,
						AccessorMemberScope.Public, out var collectionMembers);
					WalkGraph(visited, item, collectionMembers, collectionAccessor, nodes);
				}
			}
		}
	}
}