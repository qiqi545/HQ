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
using System.Diagnostics;
using HQ.Extensions.Logging;
using HQ.Test.Sdk.Assertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HQ.Test.Sdk
{
    public abstract class ServiceUnderTest : TestScope, IDisposable
    {
        private ILogger<ServiceUnderTest> _logger;

        public IAssert Assert => Should.Assert;

        protected ServiceUnderTest()
        {
            InitializeServiceProvider();

            TryInstallLogging(ServiceProvider);

            Trace.Listeners.Add(new ActionTraceListener(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            }));
        }

        protected ServiceUnderTest(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;

            TryInstallLogging(serviceProvider);

            Trace.Listeners.Add(new ActionTraceListener(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            }));
        }

        private void InitializeServiceProvider()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        public virtual void ConfigureServices(IServiceCollection services) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void TryInstallLogging(IServiceProvider serviceProvider)
        {
            var loggerFactory = serviceProvider?.GetService<ILoggerFactory>();
            loggerFactory = loggerFactory ?? DefaultLoggerFactory;
            loggerFactory.AddProvider(CreateLoggerProvider());
            _logger = loggerFactory.CreateLogger<ServiceUnderTest>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public override ILogger GetLogger()
        {
            return ServiceProvider?.GetService<ILogger<ServiceUnderTest>>() ?? _logger;
        }

        protected static IServiceProvider CreateServiceProvider(IServiceFixture fixture)
        {
            var services = new ServiceCollection();
            fixture.ConfigureServices(services);
            fixture.ServiceProvider = services.BuildServiceProvider();
            return fixture.ServiceProvider;
        }
    }
}
