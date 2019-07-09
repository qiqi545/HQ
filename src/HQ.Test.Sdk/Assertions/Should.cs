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

        public static void Be<T>(this IShould<T> @this, T other, string userMessage = null, params object[] userMessageArgs)
		{
            Assert.Equal(@this.Value, other, userMessage, userMessageArgs);
        }

        public static void NotBeNull<T>(this IShould<T> @this, string userMessage = null, params object[] userMessageArgs)
		{
			Assert.NotNull(@this.Value, userMessage, userMessageArgs);
		}

        public static void BeOk(this IShould<HttpResponseMessage> response, string userMessage = null, params object[] userMessageArgs)
        {
            Assert.NotNull(response, userMessage, userMessageArgs);
            Assert.NotNull(response.Value, userMessage, userMessageArgs);
            Assert.True(response.Value.IsSuccessStatusCode, userMessage ?? $"Response status code was {response.Value.StatusCode}", userMessageArgs);
        }

        public static void HaveStatus(this IShould<HttpResponseMessage> response, HttpStatusCode statusCode, string userMessage = null, params object[] userMessageArgs)
		{
            Assert.NotNull(response, userMessage, userMessageArgs);
			Assert.NotNull(response.Value, userMessage, userMessageArgs);
			Assert.True(response.Value.StatusCode == statusCode, userMessage, userMessageArgs);
		}

        public static void HaveHeader(this IShould<HttpResponseMessage> response, string header, string userMessage = null, params object[] userMessageArgs)
		{
	        Assert.NotNull(response, userMessage, userMessageArgs);
			Assert.NotNull(response.Value, userMessage, userMessageArgs);
			Assert.True(response.Value.Headers.Contains(header), userMessage, userMessageArgs);
		}

        public static void NotHaveHeader(this IShould<HttpResponseMessage> response, string header, string userMessage = null, params object[] userMessageArgs)
        {
	        Assert.NotNull(response, userMessage, userMessageArgs);
	        Assert.NotNull(response.Value, userMessage, userMessageArgs);
	        Assert.False(response.Value.Headers.Contains(header), userMessage, userMessageArgs);
        }

		public static void HaveBody(this IShould<HttpResponseMessage> response, string userMessage = null, params object[] userMessageArgs)
        {
            Assert.NotNull(response, userMessage, userMessageArgs);
			Assert.NotNull(response.Value, userMessage, userMessageArgs);
			Assert.True(response.Value.Content != null, userMessage, userMessageArgs);
		}

        public static void HaveBodyOfType<T>(this IShould<HttpResponseMessage> response, string userMessage = null, params object[] userMessageArgs)
		{
            Assert.NotNull(response, userMessage, userMessageArgs);
			Assert.NotNull(response.Value, userMessage, userMessageArgs);
			Assert.NotNull(response.Value.Content, userMessage, userMessageArgs);

			var json = response.Value.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            Assert.NotEmpty(json);

            var body = JsonConvert.DeserializeObject<T>(json);
            Assert.NotNull(body);
        }
    }
}
