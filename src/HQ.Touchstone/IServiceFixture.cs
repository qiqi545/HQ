using System;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Touchstone
{
    public interface IServiceFixture : IDisposable
    {
        void ConfigureServices(IServiceCollection services);
        IServiceProvider ServiceProvider { get; set; }
    }
}
