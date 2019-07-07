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

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace HQ.UI.Web
{
	/// <summary>
	///     Use <code>using static InlineElements</code> to enable inline elements anywhere they are not implicitly
	///     available.
	/// </summary>
	public static partial class InlineElements
	{
		public static string a(string value)
		{
			return $"<a>{value}</a>";
		}

		public static string abbr(string value)
		{
			return $"<abbr>{value}</abbr>";
		}

		public static string acronym(string value)
		{
			return $"<acronym>{value}</acronym>";
		}

		public static string b(string value)
		{
			return $"<b>{value}</b>";
		}

		public static string bdo(string value)
		{
			return $"<bdo>{value}</bdo>";
		}

		public static string big(string value)
		{
			return $"<big>{value}</big>";
		}

		public static string br(string value)
		{
			return $"<br>{value}</br>";
		}

		public static string cite(string value)
		{
			return $"<cite>{value}</cite>";
		}

		public static string code(string value)
		{
			return $"<code>{value}</code>";
		}

		public static string dfn(string value)
		{
			return $"<dfn>{value}</dfn>";
		}

		public static string em(string value)
		{
			return $"<em>{value}</em>";
		}

		public static string i(string value)
		{
			return $"<i>{value}</i>";
		}

		public static string img(string value)
		{
			return $"<img>{value}</img>";
		}

		public static string input(string value)
		{
			return $"<input>{value}</input>";
		}

		public static string kbd(string value)
		{
			return $"<kbd>{value}</kbd>";
		}

		public static string label(string value)
		{
			return $"<label>{value}</label>";
		}

		public static string map(string value)
		{
			return $"<map>{value}</map>";
		}

		public static string @object(string value)
		{
			return $"<object>{value}</object>";
		}

		public static string output(string value)
		{
			return $"<output>{value}</output>";
		}

		public static string q(string value)
		{
			return $"<q>{value}</q>";
		}

		public static string samp(string value)
		{
			return $"<samp>{value}</samp>";
		}

		public static string script(string value)
		{
			return $"<script>{value}</script>";
		}

		public static string select(string value)
		{
			return $"<select>{value}</select>";
		}

		public static string small(string value)
		{
			return $"<small>{value}</small>";
		}

		public static string span(string value)
		{
			return $"<span>{value}</span>";
		}

		public static string strong(string value)
		{
			return $"<strong>{value}</strong>";
		}

		public static string sub(string value)
		{
			return $"<sub>{value}</sub>";
		}

		public static string sup(string value)
		{
			return $"<sup>{value}</sup>";
		}

		public static string textarea(string value)
		{
			return $"<textarea>{value}</textarea>";
		}

		public static string time(string value)
		{
			return $"<time>{value}</time>";
		}

		public static string tt(string value)
		{
			return $"<tt>{value}</tt>";
		}

		public static string var(string value)
		{
			return $"<var>{value}</var>";
		}
	}
}