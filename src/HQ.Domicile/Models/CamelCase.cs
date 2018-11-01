// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using HQ.Common.Helpers;

namespace HQ.Domicile.Models
{
	internal class CamelCase : ITextTransform
	{
		public string Name => "Camel";

		public string Transform(string input)
		{
			return StringBuilderPool.Scoped(sb =>
			{
				if (input == null)
					return;

				sb.Append(input);

				if (string.IsNullOrWhiteSpace(input) || char.IsLower(input[0])) return;

				sb[0] = char.ToLowerInvariant(sb[0]);
			});
		}
	}
}