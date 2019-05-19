// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Blowdart.UI.Internal;

namespace Blowdart.UI.Web.SemanticUI
{
	public static class IconExtensions
	{
		private static readonly Dictionary<Enum, string> Interned = new Dictionary<Enum, string>();

		public static Ui Icon<T>(this Ui ui, T icon, bool active = false, string label = null) where T : Enum
		{
			var sb = Pools.StringBuilderPool.Get();
			try
			{
				ui.BeginI(GetCssClass<T>(icon, active, sb));
				if (!string.IsNullOrWhiteSpace(label))
					ui.Literal(label);
				ui.EndI();
				return ui;
			}
			finally
			{
				Pools.StringBuilderPool.Return(sb);
			}
		}

		private static string GetCssClass<T>(Enum icon, bool active, StringBuilder sb)
		{
			sb.AppendIconClass<T>(icon);
			if (active)
				sb.Append(" active");
			return sb.ToString();
		}

		private static void AppendIconClass<T>(this StringBuilder sb, Enum icon)
		{
			if (Interned.TryGetValue(icon, out var cssClass))
			{
				sb.Append(cssClass);
				return;
			}
			var name = Enum.GetName(typeof(T), icon);
			if (name == null)
				throw new NullReferenceException();
			foreach (var @char in name)
			{
				if (char.IsUpper(@char))
					sb.Append(' ').Append(char.ToLowerInvariant(@char));
				else
					sb.Append(@char);
			}
			sb.Append(" icon");
			cssClass = sb.ToString();
			Interned[icon] = cssClass;
		}
	}
}