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

using System;
using Xunit;
using Xunit.Sdk;

namespace HQ.Test.Sdk.Xunit
{
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer("HQ.Test.Sdk.Xunit.Extensions.TheoryDiscoverer",
#if PLATFORM
        "HQ.Platform"
#else
        "HQ.Test.Sdk.Xunit"
#endif
    )]
    public class DataDrivenTest : TheoryAttribute
    {
        public string[] Environments { get; set; }

        public string Environment
        {
            get => Environments?.Length != 1 ? null : Environments[0];
            set => Environments = new[] { value };
        }
    }
}