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
using System.IO;
using System.Linq;
using ActiveCaching;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace HQ.Extensions.Caching
{
	public class FileCacheDependency : ICacheDependency
	{
		private readonly List<string> _filters = new List<string>();
		private readonly IFileProvider _provider;

		/// <param name="fileName">
		///     The fully qualified name of the file, or the relative file name. Do not end the path with the
		///     directory separator character.
		/// </param>
		public FileCacheDependency(string fileName)
		{
			var fileInfo = new FileInfo(fileName);
			_provider = new PhysicalFileProvider(fileInfo.DirectoryName);
			_filters.Add(fileName);
		}

		/// <param name="rootDirectory">The root directory. This should be an absolute path.</param>
		/// <param name="exclusionFilters">Specifies which files or directories are excluded.</param>
		/// <param name="filters">
		///     Filter strings used to determine what files or folders to monitor. Example: **/*.cs, *.*,
		///     subFolder/**/*.cshtml.
		/// </param>
		public FileCacheDependency(string rootDirectory, ExclusionFilters exclusionFilters = ExclusionFilters.Sensitive,
			params string[] filters)
		{
			_provider = new PhysicalFileProvider(rootDirectory, exclusionFilters);
			_filters.AddRange(filters);
		}

		public FileCacheDependency(IFileProvider provider, params string[] filters)
		{
			_provider = provider;
			_filters.AddRange(filters);
		}

		public IChangeToken GetChangeToken()
		{
			switch (_filters.Count)
			{
				case 0:
					return NullChangeToken.Singleton;
				case 1:
					return _provider.Watch(_filters[0]);
				default:
					return new CompositeChangeToken(_filters.Select(f => _provider.Watch(f)).ToList());
			}
		}

		public void Dispose() { }
	}
}