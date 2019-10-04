// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Change: Uses FastConfigurationBinder to do the binding rather than the stock ConfigurationBinder.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Options
{
	/// <summary>
	///     Configures an option instance by using <see cref="FastConfigurationBinder.FastBind(IConfiguration, object)" />
	///     against an <see cref="IConfiguration" />.
	/// </summary>
	/// <typeparam name="TOptions">The type of options to bind.</typeparam>
	internal sealed class FastNamedConfigureFromConfigurationOptions<TOptions> : ConfigureNamedOptions<TOptions>
		where TOptions : class
	{
		/// <summary>
		///     Constructor that takes the <see cref="IConfiguration" /> instance to bind against.
		/// </summary>
		/// <param name="name">The name of the options instance.</param>
		/// <param name="config">The <see cref="IConfiguration" /> instance.</param>
		public FastNamedConfigureFromConfigurationOptions(string name, IConfiguration config)
			: this(name, config, _ => { })
		{
		}

		/// <summary>
		///     Constructor that takes the <see cref="IConfiguration" /> instance to bind against.
		/// </summary>
		/// <param name="name">The name of the options instance.</param>
		/// <param name="config">The <see cref="IConfiguration" /> instance.</param>
		/// <param name="configureBinder">Used to configure the <see cref="BinderOptions" />.</param>
		public FastNamedConfigureFromConfigurationOptions(string name, IConfiguration config,
			Action<BinderOptions> configureBinder)
			: base(name, options => config.FastBind(options, configureBinder))
		{
			if (config == null)
			{
				throw new ArgumentNullException(nameof(config));
			}
		}
	}
}