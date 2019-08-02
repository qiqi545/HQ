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

using HQ.Extensions.Scheduling.Hooks;

namespace HQ.Extensions.Scheduling.Models
{
    /// <summary>
    ///     Stores method hooks. Used as a cache key for running tasks.
    /// </summary>
    internal class HandlerHooks
    {
        internal Before OnBefore { get; set; }
        internal Handler Handler { get; set; }
        internal After OnAfter { get; set; }
        internal Success OnSuccess { get; set; }
        internal Failure OnFailure { get; set; }
        internal Halt OnHalt { get; set; }
        internal Error OnError { get; set; }
    }
}
