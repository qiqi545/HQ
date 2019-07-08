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
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace HQ.Test.Sdk.Extensions
{
    public static class HttpClientExtensions
    {
        private static async Task<HttpResponseMessage> SendWithoutBodyAsync(HttpClient client, string method,
	        string pathString, Action<HttpRequestMessage> configureAction)
        {
            return await SendWithoutBodyAsync(client, method, Normalize(pathString), configureAction);
        }

        private static async Task<HttpResponseMessage> SendWithBodyAsync<T>(HttpClient client, string method,
            string pathString, T body, Action<HttpRequestMessage> configureAction) where T : class
        {
            return await SendWithBodyAsync(client, method, Normalize(pathString), body, configureAction);
        }

        private static async Task<HttpResponseMessage> SendWithoutBodyAsync(HttpClient client, string method,
            PathString pathString, Action<HttpRequestMessage> configureAction)
        {
            var requestUri = pathString.ToUriComponent();
            var request = new HttpRequestMessage(new HttpMethod(method), requestUri);
            configureAction?.Invoke(request);
            return await client.SendAsync(request);
        }

        private static async Task<HttpResponseMessage> SendWithBodyAsync<T>(HttpClient client, string method,
            PathString pathString, T body, Action<HttpRequestMessage> configureAction) where T : class
        {
            var requestUri = pathString.ToUriComponent();
            var request = new HttpRequestMessage(new HttpMethod(method), requestUri)
            {
	            Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8,
		            Constants.MediaTypes.Json)
            };
            configureAction?.Invoke(request);
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

        #region Safe Methods

        public static async Task<HttpResponseMessage> TraceAsync(this HttpClient client, PathString pathString, Action<HttpRequestMessage> configureAction = null)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Trace, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> TraceAsync(this HttpClient client, string pathString, Action<HttpRequestMessage> configureAction = null)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Trace, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient client, string pathString, Action<HttpRequestMessage> configureAction = null)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Head, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> OptionsAsync(this HttpClient client, PathString pathString, Action<HttpRequestMessage> configureAction = null)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Options, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> OptionsAsync(this HttpClient client, string pathString, Action<HttpRequestMessage> configureAction = null)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Options, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> GetAsync(this HttpClient client, PathString pathString, Action<HttpRequestMessage> configureAction = null)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Get, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> GetAsync(this HttpClient client, string pathString, Action<HttpRequestMessage> configureAction)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Get, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, PathString pathString, Action<HttpRequestMessage> configureAction)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Delete, pathString, configureAction);
        }

        public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string pathString, Action<HttpRequestMessage> configureAction)
        {
            return await SendWithoutBodyAsync(client, HttpMethods.Delete, pathString, configureAction);
        }

        #endregion

        #region Unsafe Methods

        public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, PathString pathString, T body, Action<HttpRequestMessage> configureAction = null) where T : class
        {
            return await SendWithBodyAsync(client, HttpMethods.Post, pathString, body, configureAction);
        }

        public static async Task<HttpResponseMessage> PostAsync<T>(this HttpClient client, string pathString, T body, Action<HttpRequestMessage> configureAction = null)
            where T : class
        {
            return await SendWithBodyAsync(client, HttpMethods.Post, pathString, body, configureAction);
        }

        public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, PathString pathString, T body, Action<HttpRequestMessage> configureAction = null)
            where T : class
        {
            return await SendWithBodyAsync(client, HttpMethods.Put, pathString, body, configureAction);
        }

        public static async Task<HttpResponseMessage> PutAsync<T>(this HttpClient client, string pathString, T body, Action<HttpRequestMessage> configureAction = null)
            where T : class
        {
	        return await SendWithBodyAsync(client, HttpMethods.Put, pathString, body, configureAction);
        }

        public static async Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, PathString pathString, T body, Action<HttpRequestMessage> configureAction = null) where T : class
        {
            return await SendWithBodyAsync(client, HttpMethods.Patch, pathString, body, configureAction);
        }

        public static async Task<HttpResponseMessage> PatchAsync<T>(this HttpClient client, string pathString, T body, Action<HttpRequestMessage> configureAction = null)
            where T : class
        {
            return await SendWithBodyAsync(client, HttpMethods.Patch, pathString, body, configureAction);
        }

        #endregion
    }
}
