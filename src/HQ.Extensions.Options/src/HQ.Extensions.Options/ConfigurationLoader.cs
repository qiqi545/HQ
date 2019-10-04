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
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace HQ.Extensions.Options
{
	public static class ConfigurationLoader
	{
		public static IConfiguration FromEmbeddedJsonFile(string fileName)
		{
			return LoadFromEmbeddedResource(fileName, AppDomain.CurrentDomain.GetAssemblies());
		}

		public static IConfiguration FromEmbeddedJsonFile(string fileName, params Assembly[] assemblies)
		{
			return LoadFromEmbeddedResource(fileName, assemblies);
		}

		private static IConfiguration LoadFromEmbeddedResource(string fileName, Assembly[] assemblies)
		{
			var builder = new ConfigurationBuilder();
			foreach (var assembly in assemblies)
			{
				if (assembly.IsDynamic)
					continue;
				try
				{
					foreach (var resourceName in assembly.GetManifestResourceNames())
					{
						if (!resourceName.EndsWith(fileName))
							continue;
						var baseNamespace = resourceName.Replace($".{fileName}", string.Empty);
						var provider = new EmbeddedFileProvider(assembly, baseNamespace);
						builder.AddJsonFile(o =>
						{
							o.FileProvider = provider;
							o.Path = fileName;
							o.Optional = false;
							o.ReloadOnChange = false;
						});
					}
				}
				catch (Exception exception)
				{
					Console.WriteLine(exception);
					throw;
				}
			}

			IConfiguration configSeed = null;
			if (builder.Sources.Count > 0)
				configSeed = builder.Build();
			return configSeed;
		}
	}
}