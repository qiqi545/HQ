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

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace HQ.Integration.Sqlite.Sql
{
    public static class SqliteConfigurationExtensions
    {
        public static IConfigurationBuilder AddSqlite(this IConfigurationBuilder builder, string path, IConfiguration configSeed = null)
        {
            return AddSqlite(builder, provider: null, path: path, optional: false, reloadOnChange: false, configSeed: configSeed);
        }

        public static IConfigurationBuilder AddSqlite(this IConfigurationBuilder builder, string path, bool optional, IConfiguration configSeed = null)
        {
            return AddSqlite(builder, provider: null, path: path, optional: optional, reloadOnChange: false, configSeed: configSeed);
        }

        public static IConfigurationBuilder AddSqlite(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange, IConfiguration configSeed = null)
        {
            return AddSqlite(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange, configSeed: configSeed);
        }

        public static IConfigurationBuilder AddSqlite(this IConfigurationBuilder builder, IFileProvider provider, string path, bool optional, bool reloadOnChange, IConfiguration configSeed)
        {
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            var source = new SqliteConfigurationSource(path)
            {
                ReloadOnChange = reloadOnChange,
                ConfigSeed = configSeed,
                SeedStrategy = SeedStrategy.InsertIfEmpty
                // FileProvider = provider,
                // Optional = optional
            };
            builder.Add(source);
            return builder;
        }
    }
}
