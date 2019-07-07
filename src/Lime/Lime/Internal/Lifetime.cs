// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lime.Internal
{
	public enum Lifetime
	{
		AlwaysNew,
		Permanent,
		Thread,
#if SupportsRequestMemoization
		Request
#endif
	}
}