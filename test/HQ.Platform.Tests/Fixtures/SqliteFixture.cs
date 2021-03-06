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
using System.IO;
using ActiveScheduler;
using HQ.Data.SessionManagement;
using HQ.Test.Sdk;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Tests.Fixtures
{
    public abstract class SqliteFixture : IServiceFixture
    {
        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices(IServiceCollection services) { }

        public void Dispose()
        {
            var connection = ServiceProvider?.GetRequiredService<IDataConnection<BackgroundTaskBuilder>>();
            if (!(connection?.Current is WrapDbConnection wrapped) || !(wrapped.Inner is SqliteConnection sqlite))
                return;

            sqlite.Close();
            sqlite.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            if(sqlite.DataSource != null)
                File.Delete(sqlite.DataSource);
        }
    }
}
