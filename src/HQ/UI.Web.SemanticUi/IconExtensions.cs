using System;
using System.Collections.Generic;
using System.Text;
using HQ.UI.Theming;
using TypeKitchen;

namespace HQ.UI.Web.SemanticUi
{
	public static class IconExtensions
	{
		private static readonly Dictionary<Enum, string> Interned = new Dictionary<Enum, string>();

		public static Ui Icon<T>(this Ui ui, T icon, NamedColors color = NamedColors.Unspecified,
			NamedSizes size = NamedSizes.Unspecified, bool active = false, string label = null, bool fitted = true)
			where T : Enum
		{
			var sb = Pooling.StringBuilderPool.Get();
			try
			{
				ui.BeginI(sb.AppendClass<T>(icon, active, color, size, fitted));
				if (!string.IsNullOrWhiteSpace(label))
					ui.Literal(label);
				ui.EndI();
				return ui;
			}
			finally
			{
				Pooling.StringBuilderPool.Return(sb);
			}
		}

		private static string AppendClass<T>(this StringBuilder sb, Enum icon, bool active, NamedColors color,
			NamedSizes size, bool fitted)
		{
			sb.AppendEnumNameAsWord<T>(icon);
			sb.Append(" icon");
			if (active)
				sb.Append(" active");
			if (color != NamedColors.Unspecified)
				sb.AppendEnumNameAsWord<NamedColors>(color);
			if (size != NamedSizes.Unspecified)
				sb.AppendEnumNameAsWord<NamedSizes>(size);
			if (fitted)
				sb.Append(" fitted");
			return sb.ToString();
		}

		private static StringBuilder AppendEnumNameAsWord<T>(this StringBuilder sb, Enum @enum)
		{
			if (!Interned.TryGetValue(@enum, out var nameAsWord))
			{
				var wsb = Pooling.StringBuilderPool.Get();
				try
				{
					var name = Enum.GetName(typeof(T), @enum);
					if (name == null)
						throw new NullReferenceException();
					foreach (var @char in name)
						if (char.IsUpper(@char))
							wsb.Append(' ').Append(char.ToLowerInvariant(@char));
						else
							wsb.Append(@char);

					nameAsWord = wsb.ToString();
					Interned[@enum] = nameAsWord;
				}
				finally
				{
					Pooling.StringBuilderPool.Return(wsb);
				}
			}

			sb.Append(nameAsWord);
			return sb;
		}
	}
}