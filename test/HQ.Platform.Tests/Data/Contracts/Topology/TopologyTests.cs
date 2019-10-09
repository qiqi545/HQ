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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HQ.Data.Contracts.Tests.Fakes;
using HQ.Test.Sdk;

namespace HQ.Data.Contracts.Tests.Topology
{
	public class TopologyTests : UnitUnderTest
	{
		[Test]
		public void BasicTests_valid_when_no_cycles_detected()
		{
			var a = new Node("A");
			var b = new Node("B");
			var c = new Node("C");

			// C -> B -> A
			a.DependsOn(b);
			b.DependsOn(c);

			var root = new Root();
			root.Nodes.Add(a);
			root.Nodes.Add(b);
			root.Nodes.Add(c);

			var results = new List<ValidationResult>();
			var context = new ValidationContext(root, ServiceProvider, null);
			Validator.TryValidateObject(root, context, results, true);
			Assert.Empty(results, "expected no cycles");
		}

		[Test]
		public void BasicTests_invalid_when_cycles_detected()
		{
			var a = new Node("A");
			var b = new Node("B");

			a.DependsOn(b);
			b.DependsOn(a);

			var root = new Root();
			root.Nodes.Add(a);
			root.Nodes.Add(b);

			var results = new List<ValidationResult>();
			var context = new ValidationContext(root, ServiceProvider, null);
			Validator.TryValidateObject(root, context, results, true);
			Assert.NotEmpty(results, "expected a cycle");
		}
	}
}
