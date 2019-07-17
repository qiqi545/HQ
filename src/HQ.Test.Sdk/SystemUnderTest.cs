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
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HQ.Test.Sdk.Assertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HQ.Test.Sdk
{
	public class NoStartup { }

	public abstract class SystemUnderTest : SystemUnderTest<NoStartup> { }

    public abstract class SystemUnderTest<T> : ServiceUnderTest, ILogger<T> where T : class
    {
		private readonly SystemHostFixture<T> _systemUnderTest;

		protected SystemUnderTest(SystemTopology topology = SystemTopology.Web)
		{
            _systemUnderTest = new SystemHostFixture<T>(this, topology);
        }

		protected override void InitializeServiceProvider()
		{
			/* defer configuration until host startup */
		}

		public virtual void Configuration(IConfiguration config) { }

        public virtual void Configure(IApplicationBuilder app) { }

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
	        return Logger;
        }

		#region Helper Methods

		private readonly List<Action<IServiceCollection>> _configureServices = new List<Action<IServiceCollection>>();
		private readonly List<Action<IApplicationBuilder>> _configure = new List<Action<IApplicationBuilder>>();

		protected void Arrange(Action<IServiceCollection> configureServices)
		{
			lock(_systemUnderTest)
				_configureServices.Add(configureServices);
		}

		protected void Arrange(Action<IApplicationBuilder> configure)
		{
			lock (_systemUnderTest)
				_configure.Add(configure);
		}

		protected async Task Act(string pathString, Action<HttpResponseMessage> assert = null,
			Action<HttpRequestMessage> arrange = null, [CallerMemberName] string callerMemberNameOrMethod = null)
		{
			if (_configureServices.Count > 0)
			{
				lock (_systemUnderTest)
				{
					try
					{
						using (var client = _systemUnderTest.CreateClient(_configureServices))
						{
							Act(client).GetAwaiter().GetResult();
						}
					}
					finally
					{
						_configureServices.Clear();
						_configure.Clear();
					}
				}
			}
			else
			{
				using (var client = CreateClient())
				{
					await Act(client);
				}
			}

			async Task Act(HttpClient client)
			{
				var method = ResolveHttpMethod(callerMemberNameOrMethod);
				var response = await Extensions.HttpClientExtensions.SendWithoutBodyAsync(client, method, pathString, arrange);
				assert?.Invoke(response);
			}
		}

		protected async Task Act<TResponse>(string pathString, Action<TResponse> assert = null,
			Action<HttpRequestMessage> arrange = null, [CallerMemberName] string callerMemberNameOrMethod = null)
		{
			if (_configureServices.Count > 0)
			{
				using (var client = _systemUnderTest.CreateClient(_configureServices))
				{
					await Act(client);
				}
			}
			else
			{
				using (var client = CreateClient())
				{
					await Act(client);
				}
			}

			async Task Act(HttpClient client)
			{
				var method = ResolveHttpMethod(callerMemberNameOrMethod);

				var response = await Extensions.HttpClientExtensions.SendWithoutBodyAsync<TResponse>(client, method, pathString, arrange);
				if (response.Data != null)
					LogTrace(JsonConvert.SerializeObject(response.Data));
				response.Data.Should().NotBeNull();

				assert?.Invoke(response.Data);
			}
		}

		private static string ResolveHttpMethod(string callerMemberNameOrMethod)
		{
			var method = string.IsNullOrWhiteSpace(callerMemberNameOrMethod)
				? "GET"
				: callerMemberNameOrMethod.IndexOf('_') == -1
					? "GET"
					: callerMemberNameOrMethod.Substring(0, callerMemberNameOrMethod.IndexOf('_'))
						.ToUpperInvariant();

			switch (method)
			{
				case "TRACE":
				case "HEAD":
				case "OPTIONS":
				case "GET":
				case "DELETE":
				case "PUT":
				case "PATCH":
				case "POST":
					return method;
				default:
					return "GET";
			}
		}

		#endregion
	}
}
