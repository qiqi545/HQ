// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HQ.Rosetta
{
	public interface IQueryContextProvider
	{
		IEnumerable<QueryContext> Parse(HttpRequest source);
		IEnumerable<QueryContext> Parse(string source);
	}
}