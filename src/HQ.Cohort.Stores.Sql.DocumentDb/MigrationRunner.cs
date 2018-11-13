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

using System.Data.DocumentDb;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Cohort.Stores.Sql.DocumentDb
{
    public class MigrationRunner
    {
        private readonly DocumentDbConnectionStringBuilder _builder;
        private readonly DocumentClient _client;

        public MigrationRunner(string connectionString)
        {
            _builder = new DocumentDbConnectionStringBuilder(connectionString);
            _client = _builder.Build();
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_builder.Database));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    await _client.CreateDatabaseAsync(new Database {Id = _builder.Database});
                else
                    throw;
            }
        }

        public void MigrateUp()
        {
            CreateDatabaseIfNotExistsAsync().Wait();

            new CreateIdentitySchema(_client, _builder.Database).Up().Wait();
        }
    }
}
