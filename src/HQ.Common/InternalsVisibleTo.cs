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

[assembly: InternalsVisibleTo("HQ.Common.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Data.Contracts")]
[assembly: InternalsVisibleTo("HQ.Data.Sql")]
[assembly: InternalsVisibleTo("HQ.Data.Sql.Queries")]
[assembly: InternalsVisibleTo("HQ.Extensions.CodeGeneration")]
[assembly: InternalsVisibleTo("HQ.Extensions.Metrics")]
[assembly: InternalsVisibleTo("HQ.Extensions.Metrics.AspNetCore")]
[assembly: InternalsVisibleTo("HQ.Extensions.Metrics.Reporters.SignalR")]
[assembly: InternalsVisibleTo("HQ.Platform.Api")]
[assembly: InternalsVisibleTo("HQ.Platform.Identity")]
[assembly: InternalsVisibleTo("HQ.Platform.Identity.AspNetCore.Mvc")]
[assembly: InternalsVisibleTo("HQ.Platform.Identity.Stores.Sql")]
[assembly: InternalsVisibleTo("HQ.Platform.Operations")]
[assembly: InternalsVisibleTo("HQ.Platform.Schema")]
[assembly: InternalsVisibleTo("HQ.Platform.Security")]
[assembly: InternalsVisibleTo("HQ.Platform.Security.AspNetCore")]

namespace HQ.Common
{
    internal sealed class InternalsVisibleTo
    {
    }
}
