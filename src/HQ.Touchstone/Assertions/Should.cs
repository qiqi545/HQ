using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Xunit;

namespace HQ.Touchstone.Assertions
{
	public static class Should
	{
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