﻿#region LICENSE

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
using System.Buffers;
using HQ.Common.MergePatch.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HQ.Common.MergePatch.Configuration
{
	internal class JsonMergePatchOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<JsonMergePatchOptions> _options;

		public JsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
			IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			IOptions<JsonMergePatchOptions> options)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
			_jsonSerializerSettings = jsonOptions.Value.SerializerSettings;
			_charPool = charPool ?? throw new ArgumentNullException(nameof(charPool));
			_objectPoolProvider = objectPoolProvider ?? throw new ArgumentNullException(nameof(objectPoolProvider));
			_options = options;
		}

		public void Configure(MvcOptions options)
		{
			var jsonMergePatchLogger = _loggerFactory.CreateLogger<JsonMergePatchInputFormatter>();

			options.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				jsonMergePatchLogger,
				_jsonSerializerSettings,
				_charPool,
				_objectPoolProvider,
				options,
				_jsonOptions.Value,
				_options.Value));
		}
	}
}