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
using HQ.Extensions.Options;
using HQ.Integration.DocumentDb.DbProvider;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Configuration;

namespace HQ.Integration.DocumentDb.Options
{
    public static class Add
    {
		public static IConfigurationBuilder AddDocumentDb(this IConfigurationBuilder builder, string connectionString, bool reloadOnChange = false, IConfiguration configSeed =  null)
		{
			return builder.AddDocumentDb(new DocumentDbConnectionStringBuilder(connectionString).Bind, reloadOnChange, configSeed);
		}

		public static IConfigurationBuilder AddDocumentDb(this IConfigurationBuilder builder, Action<DocumentDbOptions> configureAction, bool reloadOnChange = false, IConfiguration configSeed = null)
		{
			var options = new DocumentDbOptions();
			var source = new DocumentConfigurationSource(options, configSeed, SeedStrategy.InsertIfNotExists)
			{
				ReloadOnChange = reloadOnChange
			};
			builder.Add(source);
			return builder;
		}
	}
}
