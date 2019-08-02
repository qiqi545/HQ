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

using System.Text;

namespace HQ.UI.Web.Internal
{
	internal static class StringBuilderExtensions
	{
		public static StringBuilder AppendTag(this StringBuilder sb, string el, Attributes attributes = null)
		{
			return sb.OpenBlock(el, attributes).CloseBlock(el, true);
		}

		public static StringBuilder AppendTag(this StringBuilder sb, string el, string innerText,
			Attributes attributes = null)
		{
			return innerText != null
				? sb.OpenBlock(el, attributes).Append(innerText).CloseBlock(el, true)
				: sb.OpenBlock(el, attributes).CloseBlock(el, true);
		}

		public static StringBuilder AppendTag(this StringBuilder sb, string el, Value128 id, string innerText,
			Attributes attributes = null)
		{
			return innerText != null
				? sb.OpenBlock(el, id, attributes).Append(innerText).CloseBlock(el, true)
				: sb.OpenBlock(el, id, attributes).CloseBlock(el, true);
		}

		public static StringBuilder OpenBlock(this StringBuilder sb, string el, Value128 id,
			Attributes attributes = null)
		{
			if (attributes == null)
				return sb.Append('<').Append(el).Append(" id='").Append(id).Append("'>");
			sb.Append('<').Append(el).Append(" id='").Append(id).Append("'").AppendAttributes(attributes).Append('>');
			return sb;
		}

		public static StringBuilder OpenBlock(this StringBuilder sb, string el, Attributes attributes = null)
		{
			if (attributes == null)
				return sb.Append('<').Append(el).Append('>');
			sb.Append('<').Append(el).AppendAttributes(attributes).Append('>');
			return sb;
		}

		private static StringBuilder AppendAttributes(this StringBuilder sb, Attributes attributes)
		{
			if (attributes?.Inner == null)
				return sb;

			foreach (var item in attributes.Inner)
			{
				if (item.Key == null || item.Value == null)
					continue;

				sb.Append(" ");
				sb.Append(item.Key);
				sb.Append("='");
				sb.Append(item.Value);
				sb.Append("'");
			}

			return sb;
		}

		public static StringBuilder CloseBlock(this StringBuilder sb, string el, bool open = false)
		{
			if (open)
				sb.Append('<');
			return sb.Append('/').Append(el).Append('>');
		}

		public static StringBuilder AppendEvent(this StringBuilder sb, string eventType, Value128 id)
		{
			sb.AppendLine();
			return sb.AppendLine($"maybeAddListener(\"{id}\", \"{eventType}\", document.getElementById(\"{id}\")); ");
		}
	}
}