using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.AspNetCore;
using HQ.Common.AspNetCore.Mvc;
using HQ.Common.Models;
using HQ.Data.Contracts.AspNetCore.Attributes;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Schema.Configuration;
using HQ.Data.Contracts.Schema.Models;
using HQ.Platform.Operations.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
#if NETCOREAPP2_2
using Swashbuckle.AspNetCore.Swagger;
#endif

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
#if NETCOREAPP2_2
		private readonly ISwaggerProvider _swaggerProvider;
		private readonly IOptionsMonitor<SwaggerOptions> _swaggerOptions;
#endif
		private readonly ISchemaVersionStore _schemaStore;
		private readonly IOptionsMonitor<MetaApiOptions> _metaOptions;
		
		private readonly IOptionsMonitor<SchemaOptions> _schemaOptions;

		public MetaController(
			IEnumerable<IMetaProvider> providers,
			ISchemaVersionStore schemaStore,
			IOptionsMonitor<MetaApiOptions> metaOptions,
#if NETCOREAPP2_2
			IOptionsMonitor<MvcJsonOptions> mvcOptions,
#else
			IOptionsMonitor<MvcNewtonsoftJsonOptions> mvcOptions,
#endif
#if NETCOREAPP2_2
			ISwaggerProvider swaggerProvider,
			IOptionsMonitor<SwaggerOptions> swaggerOptions,
#endif
			IOptionsMonitor<SchemaOptions> schemaOptions
			)
		{
			_providers = providers;
			_schemaStore = schemaStore;
			_metaOptions = metaOptions;

#if NETCOREAPP2_2
			_swaggerProvider = swaggerProvider;
			_swaggerOptions = swaggerOptions;
#endif
			_schemaOptions = schemaOptions;
		}
		
		[FeatureSelector]
		[HttpOptions("")]
		public IActionResult Options()
		{
			return Ok(new { data = new[] { "postman", "swagger" } });
		}

		[FeatureSelector]
		[HttpGet("postman")]
		public async Task<IActionResult> Postman([FromHeader(Name = "X-Postman-Version")] string version = "2.1.0")
		{
			if (string.IsNullOrWhiteSpace(version))
				return BadRequest();
			if (version != "2.1.0")
				return StatusCode((int) HttpStatusCode.NotImplemented);

			var baseUri = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}";
			var apiName = ResolveApplicationId();
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

			foreach (var provider in _providers)
				provider.Populate(baseUri, collection, Request.HttpContext.RequestServices);

			Response.Headers.Add("X-Postman-Version", "2.1.0");

			if (!string.IsNullOrWhiteSpace(_metaOptions.CurrentValue.Host))
			{
				foreach (var item in collection.item)
				{
					ReplaceHostRecursive(item);
				}
			}

			return Ok(collection);
		}

		private string ResolveApplicationId()
		{
			return _metaOptions.CurrentValue.ApplicationId ??		// static app identifier
			       _schemaOptions.CurrentValue.ApplicationId ??		// schema app identifier
			       Assembly.GetExecutingAssembly().GetName().Name;	// fallback
		}

		private void ReplaceHostRecursive(MetaItem item)
		{
			if (item is MetaFolder folder)
			{
				foreach (var child in folder.item)
				{
					ReplaceHostRecursive(child);
				}
			}

			if (item.request?.url != null)
			{
				item.request.url.host = _metaOptions.CurrentValue.Host.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
				item.request.url.protocol = null; // covered by host string
				item.request.url.port = null; // covered by host string
			}
		}

		[FeatureSelector]
		[HttpGet("swagger")]
		public IActionResult Swagger([FromHeader(Name = "X-Swagger-Version")] string version = "2.0")
		{
			return StatusCode((int) HttpStatusCode.NotImplemented);

			/*
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
						var serializer = new JsonSerializer();
						serializer.Serialize(writer, swagger);
					}
				}), "application/json", Encoding.UTF8);
			}
			catch (UnknownSwaggerDocument)
			{
				return NotFound();
			}
			*/
		}
	}
}
