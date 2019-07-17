using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HQ.Common.AspNetCore.Models;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Platform.Operations.Configuration;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using TypeKitchen;

namespace HQ.Platform.Operations.Controllers
{
    [Route("meta")]
    [DynamicAuthorize(typeof(MetaApiOptions))]
    [DynamicController]
    [MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
    [MetaDescription("Provides specifications and discovery for external tooling.")]
    [DisplayName("Meta")]
    [ApiExplorerSettings(IgnoreApi = false)]
	public class MetaController : Controller
    {
	    public string ApiName { get; set; } = Assembly.GetExecutingAssembly().GetName().Name;
	    public string ApiVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

		private readonly IEnumerable<IMetaProvider> _providers;
        private readonly ISwaggerProvider _swaggerProvider;
        private readonly JsonSerializer _swaggerSerializer;
        private readonly IOptions<SwaggerOptions> _swaggerOptions;

        public MetaController(
	        IEnumerable<IMetaProvider> providers,
            ISwaggerProvider swaggerProvider,
            IOptions<MvcJsonOptions> mvcOptions,
            IOptions<SwaggerOptions> swaggerOptions)
        {
            _providers = providers;

            _swaggerProvider = swaggerProvider;
            _swaggerOptions = swaggerOptions;
            _swaggerSerializer = SwaggerSerializerFactory.Create(mvcOptions);
		}

		[HttpOptions("")]
        public IActionResult Options()
        {
	        return Ok(new {data = new[] {"postman", "swagger"}});
        }

        [HttpGet("postman")]
        public IActionResult Postman([FromHeader(Name = "X-Postman-Version")] string version = "2.1.0")
        {
            if (string.IsNullOrWhiteSpace(version))
                return BadRequest();
            if(version != "2.1.0")
                return StatusCode((int)HttpStatusCode.NotImplemented);
            
            var baseUri = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}";

            // we want a stable ID so the collection doesn't clone in the user's client
            Guid collectionId;
            using (var md5 = MD5.Create())
            {
                var entropy = $"{ApiName}.{ApiVersion}";
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(entropy));
                collectionId = new Guid(hash);
            }

            // See: https://schema.getpostman.com/json/collection/v2.1.0/docs/index.html
            var collection = new MetaCollection
            {
                info = new
                {
                    name = ApiName,
                    _postman_id = collectionId,
                    description = new
                    {
                        content = "",
                        type = "text/markdown",
                        version = ApiVersion
                    },
                    version = ApiVersion,
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
                foreach (var preSerializeFilter in _swaggerOptions.Value.PreSerializeFilters)
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
