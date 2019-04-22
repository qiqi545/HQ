using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;
using Xunit;
using Xunit.Abstractions;

namespace Blowdart.UI.Tests
{
	public class CodeGeneration
	{
		private readonly ITestOutputHelper _console;

		public CodeGeneration(ITestOutputHelper console)
		{
			_console = console;
		}

		[Fact]
		public void Generate_inline_elements()
		{
			const string elements = @"<a><abbr><acronym><b><bdo><big><br><button><cite><code><dfn><em><i><img><input><kbd><label><map><object><output><q><samp><script><select><small><span><strong><sub><sup><textarea><time><tt><var>";
			var sb = new StringBuilder();
			sb.AppendLine("// Copyright (c) Blowdart, Inc. All rights reserved.");
			sb.AppendLine("// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.");
			sb.AppendLine();
			sb.AppendLine("// ReSharper disable InconsistentNaming");
			sb.AppendLine("// ReSharper disable CheckNamespace");
			sb.AppendLine();
			sb.AppendLine("public static partial class HtmlElementExtensions");
			sb.AppendLine("{");
			foreach (Match match in Regex.Matches(elements, "<\\w+>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
			{
				var element = match.Value.TrimStart('<').TrimEnd('>');

				sb.AppendLine();
				sb.AppendLine($"\tpublic static string @{element}(string value)");
				sb.AppendLine($"\t{{");
				sb.AppendLine($"\treturn $\"<{element}>{{value}}</{element}>\";");
				sb.AppendLine($"\t}}");
			}
			sb.AppendLine("}");
			
			_console.WriteLine(sb.ToString());
		}

		[Fact]
		public void Generate_block_elements_begin_end()
		{
			const string elements = @"<address><article><aside><blockquote><canvas><dd><div><dl><dt><fieldset><figcaption><figure><footer><form><h1><h2><h3><h4><h5><h6><header><hr><li><main><nav><noscript><ol><p><pre><section><table><tfoot><ul><video>";

			var sb = new StringBuilder();
			sb.AppendLine("// Copyright (c) Blowdart, Inc. All rights reserved.");
			sb.AppendLine("// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.");
			sb.AppendLine();
			sb.AppendLine("namespace Blowdart.UI.Web");
			sb.AppendLine("{");
			sb.AppendLine();
			sb.AppendLine("\tpublic static partial class HtmlExtensions");
			sb.AppendLine("\t{");
			foreach (Match match in Regex.Matches(elements, "<\\w+>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
			{
				var element = match.Value.TrimStart('<').TrimEnd('>');

				var casedName = char.ToUpperInvariant(element[0]) + element.Substring(1);

				sb.AppendLine();
				sb.AppendLine($"\t\tpublic static Ui Begin{casedName}(this Ui ui, object attr = null)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.BeginElement(\"{element}\", attr);");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\tpublic static Ui End{casedName}(this Ui ui)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.EndElement(\"{element}\");");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\t/// <summary> This call is equivalent to: \r\n\t\t///\t<code>\r\n\t\t///\t\tui.Begin{casedName}();\r\n\t\t///\t\taction();\r\n\t\t///\t\tui.End{casedName}();\r\n\t\t///\t</code>\r\n\t\t/// </summary>");
				sb.AppendLine($"\t\tpublic static Ui {casedName}(this Ui ui, object[] attr = null, Action action = null)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.Element(\"{element}\", attr, action);");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\t/// <summary> This call is equivalent to: \r\n\t\t///\t<code>\r\n\t\t///\t\tui.Begin{casedName}();\r\n\t\t///\t\taction(ui);\r\n\t\t///\t\tui.End{casedName}();\r\n\t\t///\t</code>\r\n\t\t/// </summary>");
				sb.AppendLine($"\t\tpublic static Ui {casedName}(this Ui ui, object[] attr = null, Action<Ui> action = null)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.Element(\"{element}\", attr, action);");
				sb.AppendLine($"\t\t}}");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");

			_console.WriteLine(sb.ToString());
		}
	}
}
