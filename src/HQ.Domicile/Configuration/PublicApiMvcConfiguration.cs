// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Buffers;
using HQ.Common;
using HQ.Domicile.Extensions;
using HQ.Domicile.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Domicile.Configuration
{
	internal class PublicApiMvcConfiguration : IConfigureOptions<MvcOptions>
	{
		private readonly ArrayPool<char> _charPool;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<PublicApiOptions> _options;
		private readonly JsonSerializerSettings _settings;

		public PublicApiMvcConfiguration(
			ILoggerFactory loggerFactory,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			JsonSerializerSettings settings,
			IOptions<PublicApiOptions> options)
		{
			_loggerFactory = loggerFactory;
			_charPool = charPool;
			_objectPoolProvider = objectPoolProvider;
			_settings = settings;
			_options = options;
		}

		public void Configure(MvcOptions options)
		{
			var logger = _loggerFactory.CreateLogger(HqLoggers.Formatters);
			var jsonOptions = new MvcJsonOptions();
			jsonOptions.Apply(_settings);

			options.InputFormatters.Clear();
			options.InputFormatters.Add(new JsonInputFormatter(logger, _settings, _charPool, _objectPoolProvider,
				options, jsonOptions));
			options.InputFormatters.Add(new JsonPatchInputFormatter(logger, _settings, _charPool, _objectPoolProvider,
				options, jsonOptions));

			options.OutputFormatters.Clear();
			options.OutputFormatters.Add(new JsonOutputFormatter(_settings, _charPool));
		}
	}
}