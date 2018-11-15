using System;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Common.Extensions
{
    internal static class EnumerableExtensions
    {
        public static SelfEnumerable<T> SelfEnumerate<T>(this List<T> inner)
        {
            return new SelfEnumerable<T>(inner);
        }

        public static FuncEnumerable<T, TResult> Enumerate<T, TResult>(this List<T> inner, Func<T, TResult> func)
        {
            return new FuncEnumerable<T, TResult>(inner, func);
        }
        
        /// <summary> Kahn's algorithm: https://en.wikipedia.org/wiki/Topological_sorting</summary>
        public static List<T> TopologicalSort<T>(this IEnumerable<T> nodes, ICollection<Tuple<T, T>> edges)
            where T : IEquatable<T>
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

            var sorted = new List<T>();
            var collection = nodes.Where(n => edges.All(e => !e.Item2.Equals(n)));
            var set = new HashSet<T>(collection);
            
            while (set.Any())
            {
                var node = set.First();
                set.Remove(node);
                sorted.Add(node);

                var list = edges.Where(e => e.Item1.Equals(node)).ToList();

                for (var i = 0; i < list.Count; i++)
                {
                    var e = list[i];
                    var m = e.Item2;
                    edges.Remove(e);
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        set.Add(m);
                    }
                }
            }

            return edges.Any() ? null : sorted;
        }
    }
}
