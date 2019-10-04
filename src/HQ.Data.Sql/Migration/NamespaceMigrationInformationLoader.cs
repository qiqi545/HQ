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

using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace HQ.Data.Sql.Migration
{
	public class NamespaceMigrationInformationLoader : IMigrationInformationLoader
	{
		private readonly DefaultMigrationInformationLoader _inner;
		private readonly string _namespace;
		private readonly IFilteringMigrationSource _source;

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
			{
				list.Add(entry.Version, entry);
			}

			return list;
		}
	}
}