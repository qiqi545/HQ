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
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Options
{
	public sealed class SaveOptionsManager<TOptions> : ISaveOptions<TOptions>
		where TOptions : class, new()
	{
		private readonly IConfigurationRoot _configuration;
		private readonly IOptionsMonitor<TOptions> _monitor;
		private readonly IServiceProvider _serviceProvider;

		public SaveOptionsManager(
			IConfigurationRoot configuration,
			IServiceProvider serviceProvider,
			IOptionsMonitor<TOptions> monitor)
		{
			_configuration = configuration;
			_serviceProvider = serviceProvider;
			_monitor = monitor;
		}

		public TOptions Value => _monitor.CurrentValue;

		public SaveOptionsResult TrySave(string key, Action<TOptions> mutator = null)
		{
			var found = false;
			var saved = false;

			foreach (var provider in _configuration.Providers.Reverse())
			{
				if (!(provider is ISaveConfigurationProvider saveProvider))
					continue; // this provider does not support saving

				if (!saveProvider.HasChildren(key))
					continue; // key not found in this provider

				var current = _monitor.CurrentValue;
				
				mutator?.Invoke(current);
				found = true;

				if (!current.IsValid(_serviceProvider))
					continue; // don't allow saving invalid options

				if (saveProvider.Save(key, current))
					saved = true;
			}

			if (saved)
				_configuration.Reload();
			return !found ? SaveOptionsResult.NotFound : saved ? SaveOptionsResult.Ok : SaveOptionsResult.NotModified;
		}

		public SaveOptionsResult TrySave(string key, Action mutator = null)
		{
			var found = false;
			var saved = false;

			foreach (var provider in _configuration.Providers.Reverse())
			{
				if (!(provider is ISaveConfigurationProvider saveProvider))
					continue; // this provider does not support saving

				if (!saveProvider.HasChildren(key))
					continue; // key not found in this provider

				var current = _monitor.CurrentValue;

				mutator?.Invoke();
				found = true;

				if (!current.IsValid(_serviceProvider))
					continue; // don't allow saving invalid options

				if (saveProvider.Save(key, current))
					saved = true;
			}

			if (saved)
				_configuration.Reload();

			return !found ? SaveOptionsResult.NotFound : saved ? SaveOptionsResult.Ok : SaveOptionsResult.NotModified;
		}

		public bool TryAdd(string key, Action mutator = null)
		{
			var saved = false;

			foreach (var provider in _configuration.Providers.Reverse())
			{
				if (!(provider is ISaveConfigurationProvider saveProvider))
					continue; // this provider does not support saving

				var current = _monitor.CurrentValue;

				mutator?.Invoke();

				if (!current.IsValid(_serviceProvider))
					continue; // don't allow saving invalid options

				if (saveProvider.Save(key, current))
					saved = true;
			}

			if (saved)
				_configuration.Reload();

			return saved;
		}

		public DeleteOptionsResult TryDelete(string key)
		{
			foreach (var provider in _configuration.Providers.Reverse())
			{
				if (!(provider is ISaveConfigurationProvider saveProvider))
					continue; // this provider does not support saving

				if (!saveProvider.HasChildren(key))
					continue; // key not found in this provider

				if (saveProvider.Delete(key))
				{
					_configuration.Reload();
					return DeleteOptionsResult.NoContent;
				}
			}

			return DeleteOptionsResult.NotFound;
		}

		public TOptions Get(string name)
		{
			return _monitor.Get(name);
		}
	}
}