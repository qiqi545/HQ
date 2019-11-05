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

using System.Buffers;
using System.Collections.Generic;
using System.Net.Mime;
using System.Xml;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Formatters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

#if NETCOREAPP2_2
using JsonInputFormatter = HQ.Platform.Api.Formatters.JsonInputFormatter;
using JsonOutputFormatter = HQ.Platform.Api.Formatters.JsonOutputFormatter;
using JsonPatchInputFormatter = HQ.Platform.Api.Formatters.JsonPatchInputFormatter;
#endif

namespace HQ.Platform.Api.Configuration
{
	internal class PlatformApiMvcConfiguration : IConfigureOptions<MvcOptions>
	{
		private readonly ArrayPool<char> _charPool;
		private readonly IEnumerable<IDynamicComponent> _components;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ObjectPoolProvider _objectPoolProvider;
		private readonly JsonSerializerSettings _settings;

		public PlatformApiMvcConfiguration(
			ILoggerFactory loggerFactory,
			ArrayPool<char> charPool,
			ObjectPoolProvider objectPoolProvider,
			JsonSerializerSettings settings,
			IEnumerable<IDynamicComponent> components)
		{
			_loggerFactory = loggerFactory;
			_charPool = charPool;
			_objectPoolProvider = objectPoolProvider;
			_settings = settings;
			_components = components;
		}

		public void Configure(MvcOptions options)
		{
			var logger = _loggerFactory.CreateLogger(Constants.Loggers.Formatters);

#if NETCOREAPP2_2
	        var jsonOptions = new MvcJsonOptions();
#else
			var jsonOptions = new MvcNewtonsoftJsonOptions();
#endif

			jsonOptions.Apply(_settings);

			options.InputFormatters.Clear();
			options.OutputFormatters.Clear();

			AddJson(options, logger, jsonOptions);
			AddXml(options);

			options.Conventions.Add(new DynamicComponentConvention(_components));
			options.Filters.Add(typeof(CanonicalRoutesResourceFilter));
		}

		private void AddXml(MvcOptions options)
		{
			if (string.IsNullOrEmpty(options.FormatterMappings.GetMediaTypeMappingForFormat("xml")))
			{
				options.FormatterMappings.SetMediaTypeMappingForFormat("xml", MediaTypeNames.Application.Xml);
			}

			options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter(
				new XmlWriterSettings {Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates},
				_loggerFactory));
			options.InputFormatters.Add(new XmlDataContractSerializerInputFormatter(options));
		}

		private void AddJson(MvcOptions options, ILogger logger,
#if NETCOREAPP2_2
	        MvcJsonOptions jsonOptions
#else
			MvcNewtonsoftJsonOptions jsonOptions
#endif
		)
		{
			if (string.IsNullOrEmpty(options.FormatterMappings.GetMediaTypeMappingForFormat("json")))
				options.FormatterMappings.SetMediaTypeMappingForFormat("json", MediaTypeNames.Application.Json);
#if NETCOREAPP2_2
			options.InputFormatters.Add(new JsonInputFormatter(logger, _settings, _charPool, _objectPoolProvider, options, jsonOptions));
			options.InputFormatters.Add(new JsonPatchInputFormatter(logger, _settings, _charPool, _objectPoolProvider, options, jsonOptions));
			options.OutputFormatters.Add(new JsonOutputFormatter(_settings, _charPool));
#else
			options.InputFormatters.Add(new NewtonsoftJsonInputFormatter(logger, _settings, _charPool,
				_objectPoolProvider, options, jsonOptions));
			options.InputFormatters.Add(new JsonPatchInputFormatter(logger, _settings, _charPool, _objectPoolProvider,
				options, jsonOptions));
			options.OutputFormatters.Add(new NewtonsoftJsonOutputFormatter(_settings, _charPool, options));
#endif
		}
	}
}