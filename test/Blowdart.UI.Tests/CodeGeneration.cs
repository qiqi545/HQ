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
			sb.AppendLine("/// <summary>Use <code>using static InlineElements</code> to enable inline elements anywhere they are not implicitly available.</summary>");
			sb.AppendLine("public static class InlineElements");
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
	}
}
