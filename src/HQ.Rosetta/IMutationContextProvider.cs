// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HQ.Rosetta
{
	public interface IMutationContextProvider
	{
		IEnumerable<MutationContext> Parse(HttpRequest source);
		IEnumerable<MutationContext> Parse(string source);
	}
}