using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch.Formatters;
using Newtonsoft.Json;
using System;
using System.Buffers;

namespace Morcatko.AspNetCore.JsonMergePatch.Configuration
{
	internal class JsonMergePatchOptionsSetup : IConfigureOptions<MvcOptions>
	{
		private readonly ILoggerFactory _loggerFactory;
#if NETCOREAPP2_2
		private readonly IOptions<MvcJsonOptions> _jsonOptions;
#else
		private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;
#endif
		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly ArrayPool<char> _charPool;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly IOptions<JsonMergePatchOptions> _options;

		public JsonMergePatchOptionsSetup(
			ILoggerFactory loggerFactory,
#if NETCOREAPP2_2
			IOptions<MvcJsonOptions> jsonOptions,
#else
			IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
#endif
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

#if NETCOREAPP2_2
			options.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				jsonMergePatchLogger,
				_jsonSerializerSettings,
				_charPool,
				_objectPoolProvider,
				options,
				_jsonOptions.Value,
				_options.Value));
#else
			options.InputFormatters.Insert(0, new JsonMergePatchInputFormatter(
				jsonMergePatchLogger,
				_jsonSerializerSettings,
				_charPool,
				_objectPoolProvider, 
				options,
				_jsonOptions.Value,
				_options.Value));
#endif
		}
	}
}
