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

using ActiveOptions;
using HQ.Integration.DocumentDb.SessionManagement;
using Microsoft.Extensions.Configuration;

namespace HQ.Integration.DocumentDb.Options
{
	public class DocumentConfigurationSource : IConfigurationSource
	{
		public DocumentConfigurationSource(DocumentDbOptions options, SaveConfigurationOptions saveConfig, IConfiguration configSeed = null)
		{
			SaveConfig = saveConfig;
			Options = options;
			ConfigSeed = configSeed;
		}

		public DocumentDbOptions Options { get; }

		public SaveConfigurationOptions SaveConfig { get; set; }

		public bool ReloadOnChange { get; set; }

		public IConfiguration ConfigSeed { get; set; }

		public IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			DocumentConfigurationHelper.MigrateToLatest(Options, SaveConfig, ConfigSeed,
				SaveConfig?.SeedStrategy ?? SeedStrategy.None);

			return new DocumentDbConfigurationProvider(this);
		}
	}
}