// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HQ.Common.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Cadence")]
[assembly: InternalsVisibleTo("HQ.Cadence.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Domicile")]
[assembly: InternalsVisibleTo("HQ.MissionControl")]

namespace HQ.Common
{
	internal sealed class InternalsVisibleTo
	{
	}
}