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
using Microsoft.Extensions.ObjectPool;

namespace HQ.Common
{
	internal static class TopologicalSorter<T> where T : IEquatable<T>
	{
		private static readonly ObjectPool<List<T>> Pool = new DefaultObjectPool<List<T>>(
			new DefaultPooledObjectPolicy<List<T>>());

		/// <summary> Kahn's algorithm: https://en.wikipedia.org/wiki/Topological_sorting </summary>
		public static List<T> Sort(IReadOnlyCollection<T> elements, IList<Tuple<T, T>> edges)
		{
			if (edges.Count == 0 && elements is List<T> list)
				return list;

			/*
                L ← Empty list that will contain the sorted elements
                S ← Set of all elements with no incoming edge
                while S is non-empty do
                    remove a node n from S
                    add n to tail of L
                    for each node m with an edge e from n to m do
                        remove edge e from the graph
                        if m has no other incoming edges then
                            insert m into S
                if graph has edges then
                    return error   (graph has at least one cycle)
                else 
                    return L   (a topologically sorted order)
             */

			// L ← Empty list that will contain the sorted elements
			var l = new List<T>(elements.Count);

			// S ← Set of all elements with no incoming edge
			var s = Pool.Get();
			try
			{
				foreach (var n in elements)
				{
					var all = true;
					foreach (var e in edges)
					{
						if (!e.Item2.Equals(n))
							continue;
						all = false;
						break;
					}

					if (all)
						s.Add(n);
				}

				// while S is non-empty do
				while (s.Count > 0)
				{
					// remove a node n from S
					var n = s[0];
					s.RemoveAt(0);

					// add n to tail of L
					l.Add(n);

					// for each node m with an edge e from n to m do
					//     remove edge e from the graph
					// if m has no other incoming edges then
					//     insert m into S

					// for each node m with an edge e from n to m do
					for (var j = edges.Count - 1; j >= 0; j--)
					{
						var e = edges[j];

						if (!e.Item1.Equals(n))
							continue;
						var m = e.Item2;

						// remove edge e from the graph
						edges.Remove(e);

						// if m has no other incoming edges then
						var all = true;
						for (var i = 0; i < edges.Count; i++)
						{
							var me = edges[i];
							if (!me.Item2.Equals(m))
								continue;
							all = false;
							break;
						}

						// insert m into S
						if (all)
							s.Add(m);
					}
				}

				// if graph has edges then
				//     return error(graph has at least one cycle)
				// else 
				//     return L(a topologically sorted order)
				return edges.Count > 0 ? null : l;
			}
			finally
			{
				s.Clear();
				Pool.Return(s);
			}
		}
	}
}