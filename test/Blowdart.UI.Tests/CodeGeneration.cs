using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;
using Blowdart.UI.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

			var map = GenerateAttributeMap();

			var sb = new StringBuilder();
			sb.AppendLine("// Copyright (c) Blowdart, Inc. All rights reserved.");
			sb.AppendLine("// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.");
			sb.AppendLine();
			sb.AppendLine("using System;");
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

				var qsb = new StringBuilder();

				if (map.TryGetValue(element, out var value))
				{
					AppendAttributeParameter(value, qsb);
				}

				if (map.TryGetValue("*", out value))
				{
					AppendAttributeParameter(value, qsb);
				}

				var qualified = qsb.ToString();

				sb.AppendLine();
				sb.AppendLine($"\t\t#region {element}");
				sb.AppendLine();
				sb.AppendLine($"\t\tpublic static Ui Begin{casedName}(this Ui ui,{qualified} object attr = null)");
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
				sb.AppendLine($"\t\tpublic static Ui {casedName}(this Ui ui, Action action)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.Element(\"{element}\", action);");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\t/// <summary> This call is equivalent to: \r\n\t\t///\t<code>\r\n\t\t///\t\tui.Begin{casedName}(attr);\r\n\t\t///\t\taction();\r\n\t\t///\t\tui.End{casedName}();\r\n\t\t///\t</code>\r\n\t\t/// </summary>");
				sb.AppendLine($"\t\tpublic static Ui {casedName}(this Ui ui, object attr, Action action)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.Element(\"{element}\", attr, action);");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\t/// <summary> This call is equivalent to: \r\n\t\t///\t<code>\r\n\t\t///\t\tui.Begin{casedName}();\r\n\t\t///\t\taction(ui);\r\n\t\t///\t\tui.End{casedName}();\r\n\t\t///\t</code>\r\n\t\t/// </summary>");
				sb.AppendLine($"\t\tpublic static Ui {casedName}(this Ui ui, Action<Ui> action)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.Element(\"{element}\", action);");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\t/// <summary> This call is equivalent to: \r\n\t\t///\t<code>\r\n\t\t///\t\tui.Begin{casedName}(attr);\r\n\t\t///\t\taction(ui);\r\n\t\t///\t\tui.End{casedName}();\r\n\t\t///\t</code>\r\n\t\t/// </summary>");
				sb.AppendLine($"\t\tpublic static Ui {casedName}(this Ui ui, object attr, Action<Ui> action)");
				sb.AppendLine($"\t\t{{");
				sb.AppendLine($"\t\t\treturn ui.Element(\"{element}\", attr, action);");
				sb.AppendLine($"\t\t}}");
				sb.AppendLine();
				sb.AppendLine($"\t\t#endregion");
			}
			sb.AppendLine("\t}");
			sb.AppendLine("}");

			_console.WriteLine(sb.ToString());
		}

		private static void AppendAttributeParameter(IEnumerable<KeyValuePair<string, Type>> value, StringBuilder qsb)
		{
			foreach (var entry in value)
			{
				var (key, type) = entry;

				qsb.Append(' ');

				// todo replace with type switch
				if (type == typeof(bool))
					qsb.Append("bool");
				else if (type == typeof(bool?))
					qsb.Append("bool?");
				else if (type.IsNullablePrimitive())
					qsb.Append(Nullable.GetUnderlyingType(type).Name.ToLowerInvariant());
				else if (type.IsValueType())
					qsb.Append(type.Name);
				else if (type.IsNullableValueType())
					qsb.Append($"{type.Name}?");
				else if (type.IsPrimitive)
					qsb.Append(type.Name.ToLowerInvariant());
				else
					qsb.Append(type.Name);

				qsb.Append(' ');
				qsb.Append(key);
				if (Nullable.GetUnderlyingType(type) != null)
					qsb.Append(" = null");
				qsb.Append(',');
			}
		}

		// reference: https://developer.mozilla.org/en-US/docs/Web/HTML/Attributes
		public Dictionary<string, List<KeyValuePair<string, Type>>> GenerateAttributeMap()
		{
			// TODO typing (src = URL, etc.)
			// TODO strong typing "type"?
			// TODO IDL validation? (type=foo => type=text)
			// TODO data-*


			var map = new Dictionary<string, List<KeyValuePair<string, Type>>>
			{
				{		
					//
					// Global Boolean Attributes:
					"*", new List<KeyValuePair<string, Type>>
					{
						new KeyValuePair<string, Type>("contenteditable", typeof(bool?)),
						new KeyValuePair<string, Type>("draggable", typeof(bool?)),
					}
				}
			};
			
			return map;

			const string globals = @"
accesskey,
autocapitalize,
class,
xcontextmenu,
dir,
dropzone
hidden,
id
itemprop
lang
slot
spellcheck
style
tabindex
title
translate
";



			const string regular = @"
accept:form,input
accept-charset:form
action:form,
align:applet,caption,col,colgroup,hr,iframe,img,table,tbody,td,tfoot,th,thead,tr
allow:iframe
alt:applet,area,img,input
async:script
autocomplete:form,input,textarea
autofocus:button,input,keygen,select,textarea
autoplay:audio,video
buffered:audio,video
challenge:keygen
charset:meta,scripts
cite:blockquote,del,ins,q
code:applet
codebase:applet
color:basefont,font,hr
cols:textarea
colspan:td,th
content:meta
coords:area
crossorigin:audio,img,link,script,video
csp:iframe
data:object
datetime:del,ins,time
decoding:img
default:track
defer:script
dirname:input,textarea,
download:a,area
enctype:form
enterkeyhint:textarea
for:label,output,button,fieldset,input,keygen,label,meter,object,output,progress,select,textarea
formaction:input,button
headers:td,th
height:canvas,embed,iframe,img,input,object,video
high:meter
href:a,area,base,link
hreflang:a,area,link
http-equiv:meta
icon:command
importance:iframe,img,link,script
integrity:link,script
intrinsicsize:img
inputmode:textarea,contenteditable
keytype:keygen
kind:track
label:track
language:script
loading:img,iframe
list:input
low:meter
manifest:html
max:input,meter,progress
maxlength:input,textarea
minlength:input,textarea
media:a,area,link,source,style
method:form
min:input,meter
multiple:input,select
name:button,form,fieldset,iframe,input,keygen,object,output,select,textarea,map,meta,param
novalidate:form
optimum:meter
pattern:input
ping:a,area
placeholder:input,textarea
poster:video
preload:audio,video
radiogroup:command
referrerpolicy:a,area,iframe,img,link,script
rel:a,area,link
required:input,select,textarea
rows:textarea
rowspan:td,th
sandbox:iframe
scope:th
scoped:style
selected:option
shape:a,area
size:input,select
sizes:link,img,source
span:col,colgroup
src:audio,embed,iframe,img,input,script,source,track,video
srcdoc:iframe
srclang:track
srcset:img,source
start:ol
step:input
summary:table
target:a,area,base,form
type:button,input,command,embed,object,script,source,style,menu
usemap:img,input,object
value:button,data,input,li,meter,option,progress,param
width:canvas,embed,iframe,img,input,object,video
";
			var sb = new StringBuilder();

			const string boolAttribs = @"
checked:command,input
controls:audio,video
disabled:button,command,fieldset,input,keygen,optgroup,option,select,textarea
formnovalidate:button
loop:audio,bgsound,marquee,video
muted:audio,video
open:details
readonly:input,textarea
reversed:ol
wrap:textarea
";

			

			_console.WriteLine(sb.ToString());
		}
	}
}
