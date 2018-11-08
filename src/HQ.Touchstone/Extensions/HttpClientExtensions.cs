using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HQ.Touchstone.Extensions
{
	public static class HttpClientExtensions
	{
		#region Safe Methods

		public static async Task<HttpResponseMessage> TraceAsync<T>(this HttpClient client, PathString pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Trace);
		}

		public static async Task<HttpResponseMessage> TraceAsync<T>(this HttpClient client, string pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Trace);
		}

		public static async Task<HttpResponseMessage> HeadAsync<T>(this HttpClient client, PathString pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Head);
		}

		public static async Task<HttpResponseMessage> HeadAsync<T>(this HttpClient client, string pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Head);
		}

		public static async Task<HttpResponseMessage> OptionsAsync<T>(this HttpClient client, PathString pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Options);
		}

		public static async Task<HttpResponseMessage> OptionsAsync<T>(this HttpClient client, string pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Options);
		}

		public static async Task<HttpResponseMessage> GetAsync<T>(this HttpClient client, PathString pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Get);
		}

		public static async Task<HttpResponseMessage> GetAsync<T>(this HttpClient client, string pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Get);
		}

		public static async Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, PathString pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Delete);
		}

		public static async Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string pathString) where T : class
		{
			return await SendWithoutBodyAsync(client, pathString, HttpMethods.Delete);
		}

		#endregion

		#region Unsafe Methods

		public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, PathString pathString, T body) where T : class
		{
			return await SendWithBodyAsync(client, HttpMethods.Post, pathString, body);
		}

		public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string pathString, T body) where T : class
		{
			return await SendWithBodyAsync(client, HttpMethods.Post, pathString, body);
		}

		public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, PathString pathString, T body) where T : class
		{
			return await SendWithBodyAsync(client, HttpMethods.Put, pathString, body);
		}

		public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string pathString, T body) where T : class
		{
			return await SendWithBodyAsync(client, HttpMethods.Put, pathString, body);
		}

		public static async Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, PathString pathString, T body) where T : class
		{
			return await SendWithBodyAsync(client, HttpMethods.Patch, pathString, body);
		}

		public static async Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string pathString, T body) where T : class
		{
			return await SendWithBodyAsync(client, HttpMethods.Patch, pathString, body);
		}

		#endregion

		private static async Task<HttpResponseMessage> SendWithoutBodyAsync(HttpClient client, string method, string pathString)
		{
			return await SendWithoutBodyAsync(client, method, Normalize(pathString));
		}

		private static async Task<HttpResponseMessage> SendWithBodyAsync<T>(HttpClient client, string method, string pathString, T body) where T : class
		{
			return await SendWithBodyAsync<T>(client, method, Normalize(pathString), body);
		}

		private static async Task<HttpResponseMessage> SendWithoutBodyAsync(HttpClient client, string method, PathString pathString)
		{
			var requestUri = pathString.ToUriComponent();
			var request = new HttpRequestMessage(new HttpMethod(method), requestUri);
			return await client.SendAsync(request);
		}

		private static async Task<HttpResponseMessage> SendWithBodyAsync<T>(HttpClient client, string method, PathString pathString, T body) where T : class
		{
			var requestUri = pathString.ToUriComponent();
			var request = new HttpRequestMessage(new HttpMethod(method), requestUri);
			request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypes.Json);
			return await client.SendAsync(request);
		}

		private static PathString Normalize(string pathString)
		{
			Contract.Assert(pathString != null);
			if (!pathString.StartsWith("/"))
				pathString = $"/{pathString}";
			if (!pathString.EndsWith("/"))
				pathString = $"{pathString}/";
			return pathString;
		}
	}
}
