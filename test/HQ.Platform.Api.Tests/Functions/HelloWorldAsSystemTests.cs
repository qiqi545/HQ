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
using System.Threading.Tasks;
using HQ.Platform.Api.Functions.Azure;
using HQ.Test.Sdk;

namespace HQ.Platform.Api.Tests.Functions
{
    public class HelloWorldAsSystemTests : SystemUnderTest<Startup>
    {
        private readonly HttpClient _client;

        public HelloWorldAsSystemTests() : base(SystemTopology.Functions)
        {
            _client = CreateClient();
        }

        [Test(Skip = "Not implemented")]
        public async Task With_query_string_and_body()
        {
            var response = await _client.GetAsync("/HelloWorld?name=HQ");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello, HQ", body);
        }
    }
}
