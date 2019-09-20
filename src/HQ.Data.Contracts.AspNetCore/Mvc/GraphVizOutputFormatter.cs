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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
			await context.HttpContext.Response.WriteAsync(GenerateDotGraph(GraphDirection.TopToBottom, context.Object));
		}

		public static string GenerateDotGraph(GraphDirection direction, object root)
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
					var rootType = root.GetType();
					var graphName = rootType.TryGetAttribute(true, out DisplayNameAttribute display)
						? display.DisplayName
						: rootType.Name;

					sb.AppendLine($"digraph \"{graphName}\" {{");
					sb.AppendLine($"\trankdir=\"{dir}\"");
					sb.AppendLine($"\tgraph[layout=dot,label=\"{graphName}\"];");
					sb.AppendLine($"\tnode[style=filled,shape=box];");

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
				AppendClusterStart(clusters, cb, GetClusterName(accessor.Type), GetClusterColor(accessor.Type));
			}

			var ordered = members.OrderBy(x => x.TryGetAttribute(out OrderAttribute order) ? order.Order : 0);

			foreach (var member in ordered)
			{
				if (member.TryGetAttribute(out TopologyRootAttribute _))
				{
					AppendClusterEnd(cb);
					clusters++;

					var clusterColor = GetClusterColor(member);

					AppendClusterStart(clusters, cb, GetClusterName(member), clusterColor);
				}

				if (accessor.TryGetValue(target, member.Name, out var value))
					WalkGraph(value, ref clusters, vb, nb, cb);
			}
		}

		private static string GetClusterName(MemberInfo member)
		{
			var name = member.TryGetAttribute(true, out DisplayNameAttribute display)
				? display.DisplayName ?? string.Empty
				: member.Name;
			return name;
		}

		private static string GetClusterName(AccessorMember member)
		{
			var name = member.TryGetAttribute(out DisplayNameAttribute display)
				? display.DisplayName ?? member.Name
				: member.Name;
			return name;
		}

		private static string GetClusterColor(ICustomAttributeProvider member)
		{
			var name = member.TryGetAttribute(true, out ColorAttribute color)
				? color.Color.ToRgbaHexString()
				: "blue";
			return name;
		}

		private static string GetClusterColor(AccessorMember member)
		{
			return member.TryGetAttribute(out ColorAttribute color) ? color.Color.ToRgbaHexString() : "blue";
		}

		private static void AppendClusterStart(int clusters, StringBuilder cb, string clusterName, string clusterColor)
		{
			cb.AppendLine($"\tsubgraph cluster_{clusters} {{");
			cb.AppendLine($"\t\tcolor=\"{clusterColor}\";");
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

			var ordered = collection.OrderBy(x =>
				x.GetType().TryGetAttribute(true, out OrderAttribute order) ? order.Order : 0);

			foreach (var node in ordered)
			{
				foreach (var dependent in node.Dependents)
				{
					var inCluster = _openCluster && collection.Contains(dependent);
					if (inCluster)
						cb.AppendLine($"\t\t\"{dependent.Id}\" -> \"{node.Id}\";");
					else
						vb.AppendLine($"\t\"{dependent.Id}\" -> \"{node.Id}\";");
				}

				var nodeType = node.GetType();
				var nodeColor = nodeType.TryGetAttribute(true, out ColorAttribute color)
					? color.Color.ToRgbaHexString()
					: defaultNodeColor;

				var nodeShape = nodeType.TryGetAttribute(true, out ShapeAttribute shape)
					? shape?.Name?.ToLowerInvariant() ?? "box"
					: "box";

				var nodeLabel = nodeType.TryGetAttribute(true, out DisplayNameAttribute display)
					? display.DisplayName ?? node.Id
					: node.Id;

				nb.AppendLine($"\t\"{node.Id}\" [color=\"{nodeColor}\",shape=\"{nodeShape}\",label=\"{nodeLabel}\"];");
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