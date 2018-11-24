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
            var logger = _loggerFactory.CreateLogger(Constants.Loggers.Formatters);
            var jsonOptions = new MvcJsonOptions();
            jsonOptions.Apply(_settings);

            options.InputFormatters.Clear();
            options.InputFormatters.Add(new JsonInputFormatter(logger, _settings, _charPool, _objectPoolProvider, options, jsonOptions));
            options.InputFormatters.Add(new JsonPatchInputFormatter(logger, _settings, _charPool, _objectPoolProvider, options, jsonOptions));

            options.OutputFormatters.Clear();
            options.OutputFormatters.Add(new JsonOutputFormatter(_settings, _charPool));
        }
    }
}
