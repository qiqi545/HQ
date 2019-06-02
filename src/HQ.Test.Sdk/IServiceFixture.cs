using System;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Test.Sdk
{
    public interface IServiceFixture : IDisposable
    {
        void ConfigureServices(IServiceCollection services);
        IServiceProvider ServiceProvider { get; set; }
    }
}
