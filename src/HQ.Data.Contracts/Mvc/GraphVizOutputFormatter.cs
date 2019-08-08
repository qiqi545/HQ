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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.DataAnnotations;
using HQ.Data.Contracts.Topology;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using TypeKitchen;

namespace HQ.Data.Contracts.Mvc
{
	public class GraphVizOutputFormatter : TextOutputFormatter
	{
		private static bool _openCluster;

		public GraphVizOutputFormatter()
		{
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/vnd.graphviz"));
			SupportedEncodings.Add(Encoding.UTF8);
		}

		public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
		{
			await context.HttpContext.Response.WriteAsync(GenerateDotGraph(GraphDirection.TopToBottom, context.Object,
				context.ObjectType.Name));
		}

		public static string GenerateDotGraph(GraphDirection direction, object root, string rootTypeName)
		{
			var dotGraph = Pooling.StringBuilderPool.Scoped(sb =>
			{
				var vb = Pooling.StringBuilderPool.Get();
				var nb = Pooling.StringBuilderPool.Get();
				var cb = Pooling.StringBuilderPool.Get();

				var clusters = 0;

				string dir;
				switch (direction)
				{
					case GraphDirection.LeftToRight:
						dir = "LR";
						break;
					case GraphDirection.RightToLeft:
						dir = "RL";
						break;
					case GraphDirection.TopToBottom:
						dir = "TB";
						break;
					case GraphDirection.BottomToTop:
						dir = "BT";
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
				}

				try
				{
					sb.AppendLine($"digraph \"{rootTypeName}\" {{");
					sb.AppendLine($"\trankdir=\"{dir}\"");
					sb.AppendLine($"\tgraph[layout=dot,label=\"{rootTypeName}\"];");
					sb.AppendLine("\tnode[style=filled,shape=box];");

					if (root is IEnumerable enumerable)
						foreach (var item in enumerable)
							WalkGraph(item, ref clusters, vb, nb, cb);
					else
						WalkGraph(root, ref clusters, vb, nb, cb);

					if (nb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine("\t// nodes:");
						sb.Append(nb.ToString());
					}

					if (cb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine("\t// clusters:");
						sb.Append(cb.ToString());
					}

					if (vb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine("\t// vertices:");
						sb.Append(vb.ToString());
					}

					sb.AppendLine("}");
				}
				finally
				{
					Pooling.StringBuilderPool.Return(vb);
					Pooling.StringBuilderPool.Return(nb);
				}
			});
			return dotGraph;
		}

		private static void WalkGraph(object target, ref int clusters, StringBuilder vb, StringBuilder nb,
			StringBuilder cb)
		{
			const string defaultNodeColor = @"lightgrey";

			switch (target)
			{
				case INode<string> node:
				{
					WalkNode(node, defaultNodeColor, _openCluster, vb, nb, cb);
					break;
				}

				case IEnumerable<INode<string>> nodes:
				{
					WalkNodes(nodes, defaultNodeColor, vb, nb, cb);
					break;
				}

				case IEnumerable collection:
				{
					foreach (var item in collection)
						WalkInterstitial(item, ref clusters, vb, nb, cb);
					break;
				}

				default:
				{
					WalkInterstitial(target, ref clusters, vb, nb, cb);
					break;
				}
			}

			AppendClusterEnd(cb);
		}

		private static void WalkInterstitial(object target, ref int clusters, StringBuilder vb, StringBuilder nb,
			StringBuilder cb)
		{
			if (target == null)
				return; // empty node

			var accessor = ReadAccessor.Create(target, AccessorMemberTypes.Properties, AccessorMemberScope.Public,
				out var members);

			if (accessor.Type.HasAttribute<TopologyRootAttribute>())
			{
				AppendClusterEnd(cb);
				clusters++;
				AppendClusterStart(clusters, cb, accessor.Type.Name);
			}

			foreach (var member in members)
			{
				if (member.TryGetAttribute(out TopologyRootAttribute _))
				{
					AppendClusterEnd(cb);
					clusters++;
					AppendClusterStart(clusters, cb, member.Name);
				}

				if (accessor.TryGetValue(target, member.Name, out var value))
					WalkGraph(value, ref clusters, vb, nb, cb);
			}
		}

		private static void AppendClusterStart(int clusters, StringBuilder cb, string clusterName)
		{
			cb.AppendLine($"\tsubgraph cluster_{clusters} {{");
			cb.AppendLine("\t\tcolor=\"blue\";");
			cb.AppendLine($"\t\tlabel=\"{clusterName}\";");
			_openCluster = true;
		}

		private static void AppendClusterEnd(StringBuilder cb)
		{
			if (_openCluster)
			{
				cb.AppendLine("\t}"); // end previous cluster
				_openCluster = false;
			}
		}

		private static void WalkNodes(IEnumerable<INode<string>> nodes, string defaultNodeColor, StringBuilder vb,
			StringBuilder nb, StringBuilder cb)
		{
			var collection = nodes as IList<INode<string>> ?? nodes.ToList();

			foreach (var node in collection)
			{
				foreach (var dependent in node.Dependents)
				{
					var inCluster = _openCluster && collection.Contains(dependent);
					if (inCluster)
						cb.AppendLine($"\t\t\"{dependent.Id}\" -> \"{node.Id}\";");
					else
						vb.AppendLine($"\t\"{dependent.Id}\" -> \"{node.Id}\";");
				}

				var nodeColor = node.GetType().TryGetAttribute(true, out ColorAttribute color)
					? color.Color.ToRgbHexString()
					: defaultNodeColor;

				nb.AppendLine($"\t\"{node.Id}\" [color=\"{nodeColor}\"];");
			}
		}

		private static void WalkNode(INode<string> node, string defaultNodeColor, bool inCluster, StringBuilder vb,
			StringBuilder nb, StringBuilder cb)
		{
			foreach (var dependent in node.Dependents)
				if (inCluster)
					cb.AppendLine($"\t\t\"{dependent.Id}\" -> \"{node.Id}\";");
				else
					vb.AppendLine($"\t\"{dependent.Id}\" -> \"{node.Id}\";");
			nb.AppendLine($"\t\"{node.Id}\" [color=\"{defaultNodeColor}\"];");
		}
	}
}