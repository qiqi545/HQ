using System;
using System.Net.Http;
using HQ.Touchstone.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Touchstone
{
	public abstract class SystemUnderTest<T> : IDisposable where T : class
	{
		private readonly Lazy<WebHostFixture<T>> _systemUnderTest;

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