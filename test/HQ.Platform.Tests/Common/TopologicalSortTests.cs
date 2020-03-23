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
using ActiveResolver;
using HQ.Test.Sdk;

namespace HQ.Platform.Tests.Common
{
	public class TopologicalSortTests : UnitUnderTest
	{
		[Test]
		public void BasicTests_return_self_when_nothing_to_sort()
		{
			var set = new List<string>(new [] { "A", "B", "C" });
			var ordered = set.OrderByTopology(s => Enumerable.Empty<string>());
			Assert.Same(set, ordered.AsList);
		}

		[Test]
		public void BasicTests_detect_cycle()
		{
			var set = new List<string>(new [] { "A", "B", "C" });

			// B depends on C
			// C depends on B
			Assert.Throws<InvalidOperationException>(() =>
			{
				set.OrderByTopology(x =>
				{
					switch (x)
					{
						case "B":
							return new[] {"C"};
						case "C":
							return new[] {"B"};
						default:
							return Enumerable.Empty<string>();
					}
				});
			}, "expected a cycle");
		}

		[Test]
		public void BasicTests_string_sort()
		{
			Assert.Equal("A", "A");

			var set = new List<string>(new [] { "A", "B", "C" });

			// B depends on C
			// C depends on A
			var ordered = set.OrderByTopology(x =>
			{
				switch (x)
				{
					case "C":
						return new[] {"B"};
					case "A":
						return new[] {"C"};
					default:
						return Enumerable.Empty<string>();
				}
			});

			var sb = new StringBuilder();
			foreach (var node in ordered)
				sb.Append(node);

			Assert.Equal("ACB", sb.ToString());
		}
	}
}
