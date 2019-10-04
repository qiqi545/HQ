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
	public sealed class ValidOptionsMonitor<TOptions> : IValidOptionsMonitor<TOptions> where TOptions : class, new()
	{
		private readonly IOptionsMonitor<TOptions> _inner;
		private readonly IServiceProvider _serviceProvider;

		public ValidOptionsMonitor(IOptionsMonitor<TOptions> inner, IServiceProvider serviceProvider)
		{
			_inner = inner;
			_serviceProvider = serviceProvider;
		}

		public IDisposable OnChange(Action<TOptions, string> listener)
		{
			return _inner.OnChange(listener);
		}

		public TOptions CurrentValue => Get(Microsoft.Extensions.Options.Options.DefaultName);

		public TOptions Get(string name)
		{
			return _inner.Get(name).Validate(_serviceProvider);
		}
	}
}