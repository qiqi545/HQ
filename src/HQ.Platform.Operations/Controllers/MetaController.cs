using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HQ.Common.AspNetCore.Models;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.Contracts.Schema.Models;
using HQ.Platform.Operations.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using TypeKitchen;

namespace HQ.Platform.Operations.Controllers
{
    [Route("meta")]
    [DynamicAuthorize(typeof(MetaApiOptions))]
    [DynamicController(typeof(MetaApiOptions))]
	[MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
    [MetaDescription("Provides specifications and discovery for external tooling.")]
    [DisplayName("Meta")]
    [ApiExplorerSettings(IgnoreApi = false)]
	public class MetaController : Controller
    {
	    private readonly IEnumerable<IMetaProvider> _providers;
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly ISchemaVersionStore _schemaStore;
        private readonly JsonSerializer _swaggerSerializer;
        private readonly IOptionsMonitor<SwaggerOptions> _swaggerOptions;
        private readonly IOptionsMonitor<SchemaOptions> _schemaOptions;

        public MetaController(
	        IEnumerable<IMetaProvider> providers,
            ISwaggerProvider swaggerProvider,
			ISchemaVersionStore schemaStore,
	        IOptionsMonitor<MvcJsonOptions> mvcOptions,
	        IOptionsMonitor<SwaggerOptions> swaggerOptions,
	        IOptionsMonitor<SchemaOptions> schemaOptions)
        {
            _providers = providers;

            _swaggerProvider = swaggerProvider;
            _schemaStore = schemaStore;
            _swaggerOptions = swaggerOptions;
            _schemaOptions = schemaOptions;
            _swaggerSerializer = SetSwaggerSerializer(mvcOptions.CurrentValue);
            mvcOptions.OnChange((o, l) => SetSwaggerSerializer(o));
        }

        private static JsonSerializer SetSwaggerSerializer(MvcJsonOptions options)
        {
	        return SwaggerSerializerFactory.Create(Microsoft.Extensions.Options.Options.Create(options));
        }

        [HttpOptions("")]
        public IActionResult Options()
        {
	        return Ok(new {data = new[] {"postman", "swagger"}});
        }

        [HttpGet("postman")]
        public async Task<IActionResult> Postman([FromHeader(Name = "X-Postman-Version")] string version = "2.1.0")
        {
            if (string.IsNullOrWhiteSpace(version))
                return BadRequest();
            if(version != "2.1.0")
                return StatusCode((int)HttpStatusCode.NotImplemented);
            
            var baseUri = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}";

            var apiName = _schemaOptions.CurrentValue.ApplicationId ?? Assembly.GetExecutingAssembly().GetName().Name;;
            var apiVersion = (await _schemaStore.GetByApplicationId(apiName)).FirstOrDefault()?.Revision ?? 0;
            var apiVersionString = apiVersion == 0 ? Assembly.GetExecutingAssembly().GetName().Version?.ToString() : apiVersion.ToString();

			// we want a stable ID so the collection doesn't clone in the user's client
			Guid collectionId;
            using (var md5 = MD5.Create())
            {
                var entropy = $"{apiName}.{apiVersion}";
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(entropy));
                collectionId = new Guid(hash);
            }

            // See: https://schema.getpostman.com/json/collection/v2.1.0/docs/index.html
            var collection = new MetaCollection
            {
                info = new
                {
                    name = apiName,
                    _postman_id = collectionId,
                    description = new
                    {
                        content = "",
                        type = "text/markdown",
                        version = apiVersionString
					},
                    version = apiVersionString,
                    schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
                },
                item = new List<MetaItem>(),
                @event = new List<dynamic>(),
                variable = new List<dynamic>(),
                auth = "bearer",
                protocolProfileBehavior = new { }
            };

            foreach(var provider in _providers)
                provider.Populate(baseUri, collection);

            Response.Headers.Add("X-Postman-Version", "2.1.0");

            return Ok(collection);
        }

        [HttpGet("swagger")]
        public IActionResult Swagger([FromHeader(Name = "X-Swagger-Version")] string version = "2.0")
        {
            var basePath = string.IsNullOrEmpty(Request.PathBase) ? null : Request.PathBase.ToString();
            try
            {
                var swagger = _swaggerProvider.GetSwagger("swagger", null, basePath);
                foreach (var preSerializeFilter in _swaggerOptions.CurrentValue.PreSerializeFilters)
                    preSerializeFilter(swagger, Request);

                Response.Headers.Add("X-Swagger-Version", "2.0");

                return Content(Pooling.StringBuilderPool.Scoped(sb =>
                {
                    using (var writer = new StringWriter(sb))
                    {
                        _swaggerSerializer.Serialize(writer, swagger);
                    }
                }), "application/json", Encoding.UTF8);
            }
            catch (UnknownSwaggerDocument)
            {
                return NotFound();
            }
        }
    }
}
