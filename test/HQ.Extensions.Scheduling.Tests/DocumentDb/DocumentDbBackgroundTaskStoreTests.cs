using HQ.Extensions.Scheduling.Tests.DocumentDb;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Extensions.Scheduling.Tests.DocumentDb
{
	[TestEnvironment(Environment = "IntegrationTests")]
    public class DocumentDbBackgroundTaskStoreTests : BackgroundTaskStoreTests, IClassFixture<SchedulingDocumentDbFixture>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public DocumentDbBackgroundTaskStoreTests(SchedulingDocumentDbFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}
