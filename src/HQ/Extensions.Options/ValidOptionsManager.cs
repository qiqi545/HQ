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
using HQ.Extensions.Options.Internal;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Options
{
	public sealed class ValidOptionsManager<TOptions> : IValidOptions<TOptions>, IValidOptionsSnapshot<TOptions>
        where TOptions : class, new()
    {
        private readonly IOptions<TOptions> _default;
        private readonly IOptionsSnapshot<TOptions> _snapshot;
        private readonly IServiceProvider _serviceProvider;

        public ValidOptionsManager(IOptions<TOptions> @default, IOptionsSnapshot<TOptions> snapshot, IServiceProvider serviceProvider)
        {
            _default = @default;
            _snapshot = snapshot;
            _serviceProvider = serviceProvider;
        }

        public TOptions Value => _default.Value;

        public TOptions Get(string name)
        {
            return _snapshot.Get(name).Validate(_serviceProvider);
        }
    }
}
