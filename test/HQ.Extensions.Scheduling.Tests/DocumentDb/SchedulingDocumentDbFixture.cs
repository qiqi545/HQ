using System;
using HQ.Integration.DocumentDb.DbProvider;
using HQ.Integration.DocumentDb.Scheduling;
using HQ.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Scheduling.Tests.DocumentDb
{
    public class SchedulingDocumentDbFixture : DocumentDbFixture
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // WARNING: this currently points to local emulator storage
            const string connectionString = "AccountEndpoint=https://localhost:8081/;" +
                                            "AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;" +
                                            "Database=Tests;";

            var builder = new DocumentDbConnectionStringBuilder(connectionString)
            {
                DefaultCollection = $"{Guid.NewGuid().ToString("N").ToUpperInvariant()}"
            };

            services.AddBackgroundTasks(o => { })
                .AddDocumentDbBackgroundTasksStore(o =>
                {
                    o.AuthKey = builder.AccountKey;
                    o.Endpoint = builder.AccountEndpoint.ToString();
                    o.DatabaseId = builder.Database;
                    o.CollectionId = builder.DefaultCollection;
                    o.SharedCollection = true;
                    o.OfferThroughput = 400;
                });
        }
    }
}
