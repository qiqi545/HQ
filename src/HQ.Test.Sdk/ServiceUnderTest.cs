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
using Microsoft.Extensions.Logging.TraceSource;

namespace HQ.Test.Sdk
{
    public abstract class ServiceUnderTest : TestScope, IDisposable
    {
        protected ILogger<ServiceUnderTest> Logger;

        public IAssert Assert => Should.Assert;

        protected ServiceUnderTest() : this(null)
        {
            InitializeServiceProvider();

            TryInstallLogging();

            TryInstallTracing();
        }

        protected ServiceUnderTest(IServiceProvider serviceProvider)
        {
            if(serviceProvider == null)
                InitializeServiceProvider();
            else
                ServiceProvider = serviceProvider;

            TryInstallLogging();

            TryInstallTracing();
        }

        public virtual void ConfigureServices(IServiceCollection services) { }
        
        private void InitializeServiceProvider()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        protected void TryInstallLogging()
        {
            if (ServiceProvider?.GetService<ILoggerFactory>() != null)
                return;

            var loggerFactory = ServiceProvider?.GetService<ILoggerFactory>();
            loggerFactory = loggerFactory ?? DefaultLoggerFactory;
            loggerFactory.AddProvider(CreateLoggerProvider());
            Logger = loggerFactory.CreateLogger<ServiceUnderTest>();
        }

        protected void TryInstallTracing()
        {
            if (ServiceProvider?.GetService<TraceSourceLoggerProvider>() != null)
                return;

            Trace.Listeners.Add(new ActionTraceListener(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            }));
        }

        public override ILogger GetLogger()
        {
            return ServiceProvider?.GetService<ILogger<ServiceUnderTest>>() ?? Logger;
        }

        protected static IServiceProvider CreateServiceProvider(IServiceFixture fixture)
        {
            var services = new ServiceCollection();
            fixture.ConfigureServices(services);
            fixture.ServiceProvider = services.BuildServiceProvider();
            return fixture.ServiceProvider;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
