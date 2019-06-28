using HQ.Extensions.Scheduling.Tests.DocumentDb;
using Xunit;

namespace HQ.Extensions.Scheduling.Tests.Sqlite
{
    public class DocumentDbBackgroundTaskStoreTests : BackgroundTaskStoreTests, IClassFixture<SchedulingDocumentDbFixture>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public DocumentDbBackgroundTaskStoreTests(SchedulingDocumentDbFixture fixture) : base(CreateServiceProvider(fixture)) { }
    }
}
