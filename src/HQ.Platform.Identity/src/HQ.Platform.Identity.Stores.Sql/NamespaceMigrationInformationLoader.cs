using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace HQ.Platform.Identity.Stores.Sql
{
    public class NamespaceMigrationInformationLoader : IMigrationInformationLoader
    {
        private readonly string _namespace;
        private readonly IFilteringMigrationSource _source;
        private readonly DefaultMigrationInformationLoader _inner;

        public NamespaceMigrationInformationLoader(string @namespace,
            IFilteringMigrationSource source, DefaultMigrationInformationLoader inner)
        {
            _namespace = @namespace;
            _source = source;
            _inner = inner;
        }

        public SortedList<long, IMigrationInfo> LoadMigrations()
        {
            var migrations =
                _source.GetMigrations(type => type.Namespace == _namespace)
                    .Select(_inner.Conventions.GetMigrationInfoForMigration);

            var list = new SortedList<long, IMigrationInfo>();
            foreach (var entry in migrations)
                list.Add(entry.Version, entry);

            return list;
        }
    }
}
