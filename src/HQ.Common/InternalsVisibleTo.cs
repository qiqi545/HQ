#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HQ.Cohort")]
[assembly: InternalsVisibleTo("HQ.Common.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Cadence")]
[assembly: InternalsVisibleTo("HQ.Cadence.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Domicile")]
[assembly: InternalsVisibleTo("HQ.InteractionTests")]
[assembly: InternalsVisibleTo("HQ.Lingo")]
[assembly: InternalsVisibleTo("HQ.MissionControl")]
[assembly: InternalsVisibleTo("HQ.Remix")]
[assembly: InternalsVisibleTo("HQ.Rosetta")]
[assembly: InternalsVisibleTo("HQ.Zero")]

namespace HQ.Common
{
	internal sealed class InternalsVisibleTo
	{
	}
}