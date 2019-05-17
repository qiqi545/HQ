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
using Microsoft.Extensions.Primitives;

namespace HQ.Common
{
    public static class EnumerableExtensions
    {
        public static SelfEnumerable<T> SelfEnumerate<T>(this List<T> inner)
        {
            return new SelfEnumerable<T>(inner);
        }

        public static FuncEnumerable<T, TResult> Enumerate<T, TResult>(this List<T> inner, Func<T, TResult> func)
        {
            return new FuncEnumerable<T, TResult>(inner, func);
        }

        public static PredicateEnumerable<T> Enumerate<T>(this List<T> inner, Predicate<T> predicate)
        {
            return new PredicateEnumerable<T>(inner, predicate);
        }

        /// <summary> Kahn's algorithm: https://en.wikipedia.org/wiki/Topological_sorting</summary>
        public static List<T> TopologicalSort<T>(this List<T> nodes, List<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            /*
                L ← Empty list that will contain the sorted elements
                S ← Set of all nodes with no incoming edge
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

            var sorted = new List<T>(nodes.Count);
            var set = new HashSet<T>();

            foreach (var node in nodes.Enumerate<T>(n => All(edges, n)))
                set.Add(node);

            while (set.Count > 0)
            {
                var node = set.ElementAt(0);
                set.Remove(node);
                sorted.Add(node);

                foreach (var e in edges.Enumerate<Tuple<T, T>>(e => e.Item1.Equals(node)))
                {
                    var m = e.Item2;
                    edges.Remove(e);

                    var all = true;
                    foreach (var me in edges)
                    {
                        if (!me.Item2.Equals(m))
                            continue;
                        all = false;
                        break;
                    }

                    if (all) set.Add(m);
                }
            }

            return edges.Count > 0 ? null : sorted;
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private static bool All<T>(List<Tuple<T, T>> edges, T n) where T : IEquatable<T>
        {
            var all = true;
            foreach (var e in edges)
            {
                if (!e.Item2.Equals(n))
                    continue;
                all = false;
                break;
            }

            return all;
        }

        public static List<T> MaybeList<T>(this IEnumerable<T> enumerable)
        {
            return enumerable as List<T> ?? enumerable.ToList();
        }

        public static bool Contains(this StringValues stringValues, string input, StringComparison comparison = StringComparison.CurrentCulture)
        {
            foreach (var value in stringValues)
            {
                if (value.Equals(input, comparison))
                    return true;
            }
            return false;
        }
    }
}
