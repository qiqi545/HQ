using System.Data.DocumentDb;
using HQ.Extensions.Scheduling.DocumentDb;
using HQ.Test.Sdk.Data;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Extensions.Scheduling.Tests.DocumentDb
{
    public class SchedulingDocumentDbFixture : DocumentDbFixture
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // WARNING: this currently points to local emulator storage
            var connectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            var connectionStringBuilder = new DocumentDbConnectionStringBuilder(connectionString);
            
            services.AddBackgroundTasks(o => { })
                .AddDocumentDbBackgroundTasksStore(connectionString, o =>
                {
                    o.AuthKey = connectionStringBuilder.AccountKey;
                    o.Endpoint = connectionStringBuilder.AccountEndpoint.ToString();
                    o.DatabaseId = "Shared";
                    o.CollectionId = "BackgroundTasks";
                    o.SharedCollection = true;
                    o.OfferThroughput = 400;
                });
        }
    }
}
