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

using System.Diagnostics;
using System.Drawing;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.DataAnnotations;
using HQ.Data.Contracts.Mvc;
using HQ.Data.Contracts.Tests.Fakes;
using HQ.Test.Sdk;

namespace HQ.Data.Contracts.Tests.Mvc
{
	public class GraphVizOutputFormatterTests : UnitUnderTest
	{
		[Test]
		public void Can_generate_topology_graph()
		{
			var a = new Node("A");
			var b = new Node("B");
			var c = new Node("C");

			// C -> B -> A
			a.DependsOn(b);
			b.DependsOn(c);

			var topology = new Topology();
			
			topology.FirstRoot.Nodes.Add(a);
			topology.FirstRoot.Nodes.Add(b);
			topology.FirstRoot.Nodes.Add(c);

			var d = new D("D");
			var e = new E("E");
			var f = new F("F");

			// F -> E -> D
			d.DependsOn(e);
			e.DependsOn(f);

			d.DependsOn(a);
			b.DependsOn(e);
			a.DependsOn(f);

			topology.SecondRoot.Nodes.Add(d);
			topology.SecondRoot.Nodes.Add(e);
			topology.SecondRoot.Nodes.Add(f);

			var dotGraph = GraphVizOutputFormatter.GenerateDotGraph(GraphDirection.TopToBottom, topology, "Topology");
			Assert.NotNull(dotGraph);
			Trace.WriteLine(dotGraph);
		}

		[Color(nameof(Color.Purple))]
		public class D : Node
		{
			public D(string id) : base(id) { }
		}

		[Color(nameof(Color.MediumPurple))]
		public class E : Node
		{
			public E(string id) : base(id) { }
		}

		[Color(nameof(Color.Plum))]
		public class F : Node
		{
			public F(string id) : base(id) { }
		}

		public class Topology
		{
			[TopologyRoot]
			public Graph FirstRoot { get; set; } = new Graph();

			[TopologyRoot]
			public Graph SecondRoot { get; set; } = new Graph();
		}
	}
}
