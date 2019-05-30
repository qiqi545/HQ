// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Lime.Tests")]
[assembly: InternalsVisibleTo("Lime.Web")]
[assembly: InternalsVisibleTo("Lime.Web.SemanticUI")]
[assembly: InternalsVisibleTo("Lime.iOS")]

namespace Lime.Internal
{
	internal sealed class InternalsVisibleTo
	{
	}
}