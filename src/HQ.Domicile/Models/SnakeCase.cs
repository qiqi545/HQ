// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Common.Helpers;

namespace HQ.Domicile.Models
{
	internal class SnakeCase : ITextTransform
	{
		public string Name => "Snake";

		public string Transform(string input)
		{
			if (string.IsNullOrEmpty(input) || input.Length > 0 && char.IsLower(input[0])) return input;

			return StringBuilderPool.Scoped(sb =>
			{
				sb.Append(char.ToLowerInvariant(input[0]));
				for (var i = 1; i < input.Length; i++)
					if (char.IsLower(input[i]))
					{
						sb.Append(input[i]);
					}
					else
					{
						sb.Append('_');
						sb.Append(char.ToLowerInvariant(input[i]));
					}
			});
		}
	}
}