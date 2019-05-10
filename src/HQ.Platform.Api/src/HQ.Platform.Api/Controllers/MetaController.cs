using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HQ.Platform.Api.Configuration;
using System.Net;
using HQ.Platform.Api.Models;

namespace HQ.Platform.Api.Controllers
{
    [Route("meta")]
    public class MetaController : Controller
    {
        private readonly IOptions<PublicApiOptions> _options;
        private readonly IEnumerable<IMetaProvider> _providers;

        public MetaController(IOptions<PublicApiOptions> options, IEnumerable<IMetaProvider> providers)
        {
            _options = options;
            _providers = providers;
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
                var entropy = $"{_options.Value.ApiName}.{_options.Value.ApiVersion}";
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(entropy));
                collectionId = new Guid(hash);
            }

            // See: https://schema.getpostman.com/json/collection/v2.1.0/docs/index.html
            var collection = new MetaCollection
            {
                info = new
                {
                    name = _options.Value.ApiName,
                    _postman_id = collectionId,
                    description = new
                    {
                        content = "",
                        type = "text/markdown",
                        version = _options.Value.ApiVersion
                    },
                    version = _options.Value.ApiVersion,
                    schema = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
                },
                item = new List<dynamic>(),
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
    }

    public class MetaCollection
    {
        public dynamic info;
        public string auth;
        public dynamic protocolProfileBehavior;
        public List<dynamic> item = new List<dynamic>();
        public List<dynamic> @event = new List<dynamic>();
        public List<dynamic> variable = new List<dynamic>();
    }
}
