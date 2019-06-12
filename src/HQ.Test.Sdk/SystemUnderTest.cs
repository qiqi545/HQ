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

using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HQ.Test.Sdk
{
    public abstract class SystemUnderTest<T> : ServiceUnderTest, ILogger<T> where T : class
    {
        private readonly SystemHostFixture<T> _systemUnderTest;
        
        protected SystemUnderTest(SystemTopology topology = SystemTopology.Web)
        {
            _systemUnderTest = new SystemHostFixture<T>(this, topology);
        }
        
        public virtual void Configuration(IConfiguration config) { }

        public virtual void Configure(IApplicationBuilder app)
        {
            ServiceProvider = app.ApplicationServices;

            TryInstallLogging();

            TryInstallTracing();
        }

        public HttpClient CreateClient()
        {
            return _systemUnderTest.CreateClient();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _systemUnderTest?.Dispose();
        }
        
        public override ILogger GetLogger()
        {
            return ServiceProvider.GetService<ILogger<SystemUnderTest<T>>>() ?? Logger;
        }
    }
}