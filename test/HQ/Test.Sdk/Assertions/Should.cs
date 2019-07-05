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

using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace HQ.Test.Sdk.Assertions
{
    public static class Should
    {
        public static IAssert Assert { get; set; }

        public static void Be<T>(this IShould<T> @this, T other)
        {
            Assert.Equal(@this.Value, other);
        }

        public static void BeOk(this IShould<HttpResponseMessage> response)
        {
            Assert.NotNull(response);
            Assert.NotNull(response.Value);
            Assert.True(response.Value.IsSuccessStatusCode);
        }

        public static void HaveStatus(this IShould<HttpResponseMessage> response, HttpStatusCode statusCode)
        {
            Assert.NotNull(response);
            Assert.NotNull(response.Value);
            Assert.True(response.Value.StatusCode == statusCode);
        }

        public static void HaveBody(this IShould<HttpResponseMessage> response)
        {
            Assert.NotNull(response);
            Assert.NotNull(response.Value);
            Assert.True(response.Value.Content != null);
        }

        public static void HaveBodyOfType<T>(this IShould<HttpResponseMessage> response)
        {
            Assert.NotNull(response);
            Assert.NotNull(response.Value);
            Assert.NotNull(response.Value.Content);

            var json = response.Value.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.NotEmpty(json);

            var body = JsonConvert.DeserializeObject<T>(json);
            Assert.NotNull(body);
        }
    }
}
