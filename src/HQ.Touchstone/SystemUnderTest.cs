using System;
using System.Diagnostics;
using System.Net.Http;
using HQ.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace HQ.Touchstone
{
	public abstract class SystemUnderTest<T> : IDisposable where T : class
	{
		private readonly Lazy<WebHostFixture<T>> _systemUnderTest;
		private ILogger<SystemUnderTest<T>> _logger;

		protected SystemUnderTest(ITestOutputHelper output)
		{
			var loggerFactory = new LoggerFactory();
			loggerFactory.AddProvider(new ActionLoggerProvider(output.WriteLine));
			_logger = loggerFactory.CreateLogger<SystemUnderTest<T>>();

			Trace.Listeners.Add(new ActionTraceListener(output.WriteLine));
		}

		public virtual void Configuration(IConfiguration config) { }

		public virtual void ConfigureServices(IServiceCollection services) { }

		public virtual void Configure(IApplicationBuilder app) { }

		public HttpClient CreateClient()
		{
			return _systemUnderTest.Value.CreateClient();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected SystemUnderTest()
		{
			_systemUnderTest = new Lazy<WebHostFixture<T>>(() =>
			{
				Bootstrap.EnsureInitialized();
				return new WebHostFixture<T>(this);
			});
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _systemUnderTest.IsValueCreated)
				_systemUnderTest.Value?.Dispose();
		}
	}
}