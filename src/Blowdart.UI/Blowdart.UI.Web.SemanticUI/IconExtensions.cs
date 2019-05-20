// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Blowdart.UI.Internal;
using Blowdart.UI.Theming;

namespace Blowdart.UI.Web.SemanticUI
{
	public static class IconExtensions
	{
		
		public static Ui Icon<T>(this Ui ui, T icon, NamedColors color = NamedColors.Unspecified, bool active = false, string label = null) where T : Enum
		{
			var sb = Pools.StringBuilderPool.Get();
			try
			{
				ui.BeginI(sb.AppendClass<T>(icon, active, color));
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

		private static string AppendClass<T>(this StringBuilder sb, Enum icon, bool active, NamedColors color)
		{
			sb.AppendEnumNameAsWord<T>(icon);
			sb.Append(" icon");
			if (active)
				sb.Append(" active");
			if(color != NamedColors.Unspecified)
				sb.AppendEnumNameAsWord<NamedColors>(color);
			return sb.ToString();
		}

		private static readonly Dictionary<Enum, string> Interned = new Dictionary<Enum, string>();

		private static StringBuilder AppendEnumNameAsWord<T>(this StringBuilder sb, Enum @enum)
		{
			if (!Interned.TryGetValue(@enum, out var nameAsWord))
			{
				var wsb = Pools.StringBuilderPool.Get();
				try
				{
					var name = Enum.GetName(typeof(T), @enum);
					if (name == null)
						throw new NullReferenceException();
					foreach (var @char in name)
					{
						if (char.IsUpper(@char))
							wsb.Append(' ').Append(char.ToLowerInvariant(@char));
						else
							wsb.Append(@char);
					}

					nameAsWord = wsb.ToString();
					Interned[@enum] = nameAsWord;
				}
				finally
				{
					Pools.StringBuilderPool.Return(wsb);
				}
			}
			sb.Append(nameAsWord);
			return sb;
		}
	}
}