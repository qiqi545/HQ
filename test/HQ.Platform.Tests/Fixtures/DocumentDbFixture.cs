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
using HQ.Integration.DocumentDb.SessionManagement;
using HQ.Test.Sdk;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Platform.Tests.Fixtures
{
	public abstract class DocumentDbFixture : IServiceFixture
    {
        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices(IServiceCollection services) { }

        public void Dispose()
        {
            var options = ServiceProvider.GetRequiredService<IOptions<DocumentDbOptions>>();
            using var client = new DocumentClient(options.Value.AccountEndpoint, options.Value.AccountKey, JsonConvert.DefaultSettings());
            client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(options.Value.DatabaseId, options.Value.CollectionId)).GetAwaiter().GetResult();
        }
    }
}
