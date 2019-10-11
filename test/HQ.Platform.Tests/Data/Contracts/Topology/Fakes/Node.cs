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
using HQ.Data.Contracts.Topology;

namespace HQ.Platform.Tests.Data.Contracts.Topology.Fakes
{
	public class Node : INode<string>, IEquatable<Node>
	{
		public Node(string id)
		{
			Id = id;
		}

		public string Id { get; }

		public void DependsOn(INode<string> node)
		{
			_nodes.Add(node);
		}

		private readonly List<INode<string>> _nodes = new List<INode<string>>();
		public IEnumerable<INode<string>> Dependents => _nodes;

		public bool Equals(Node other)
		{
			return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || string.Equals(Id, other.Id));
		}

		public bool Equals(INode<string> other)
		{
			return Id == other?.Id;
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || obj is Node other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (Id != null ? Id.GetHashCode() : 0);
		}

		public static bool operator ==(Node left, Node right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Node left, Node right)
		{
			return !Equals(left, right);
		}
	}
}