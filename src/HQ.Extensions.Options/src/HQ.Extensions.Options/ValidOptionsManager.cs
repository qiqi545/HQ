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
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Options
{
	public sealed class ValidOptionsManager<TOptions> : IValidOptions<TOptions>, IValidOptionsSnapshot<TOptions>
        where TOptions : class, new()
    {
		private readonly IOptionsFactory<TOptions> _factory;
		private readonly IServiceProvider _serviceProvider;

        public ValidOptionsManager(IOptionsFactory<TOptions> factory, IServiceProvider serviceProvider)
        {
            _factory = factory;
            _serviceProvider = serviceProvider;
        }
        private readonly OptionsCache<TOptions> _cache = new OptionsCache<TOptions>();
        
		public TOptions Value => Get(Microsoft.Extensions.Options.Options.DefaultName);

		public TOptions Get(string name)
        {
	        return _cache.GetOrAdd(name, () => _factory.Create(name)).Validate(_serviceProvider);
        }
    }
}
