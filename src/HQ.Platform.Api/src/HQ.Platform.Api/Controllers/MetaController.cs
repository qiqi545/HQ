using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using HQ.Platform.Api.Configuration;
using System.Net;

namespace HQ.Platform.Api.Controllers
{
    [Route("meta")]
    public class MetaController : Controller
    {
        private readonly IApiDescriptionGroupCollectionProvider _explorer;
        private readonly IOptions<PublicApiOptions> _options;

        public MetaController(IApiDescriptionGroupCollectionProvider apiExplorer,
            IOptions<PublicApiOptions> options)
        {
            _explorer = apiExplorer;
            _options = options;
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
            var collection = new
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

            var descriptions = _explorer.ApiDescriptionGroups.Items.SelectMany(x => x.Items);

            var manifest = descriptions.GroupBy(x => ((ControllerActionDescriptor)x.ActionDescriptor).ControllerTypeInfo);

            foreach (var group in manifest)
            {
                var controllerName = group.Key.Name.Replace(nameof(Controller), string.Empty);

                var folder = new
                {
                    name = controllerName,
                    description = "",
                    variable = new List<dynamic>(),
                    item = new List<dynamic>(),
                    @event = new List<dynamic>(),
                    auth = "bearer",
                    protocolProfileBehavior = new { }
                };

                foreach (var operation in group.OrderBy(x => x.RelativePath).ThenBy(x => x.HttpMethod))
                {
                    var url = $"{baseUri}/{operation.RelativePath}";

                    var item = new
                    {
                        id = Guid.NewGuid(),
                        name = operation.RelativePath,
                        description = "",
                        variable = new List<dynamic>(),
                        @event = new List<dynamic>(),
                        request = new
                        {
                            url,
                            auth = "bearer",
                            proxy = new { },
                            certificate = new { },
                            method = operation.HttpMethod,
                            description = new
                            {
                                content = "",
                                type = "text/markdown",
                                version = _options.Value.ApiVersion
                            },
                            header = new List<dynamic>
                            {
                                new
                                {
                                    key = "Content-Type",
                                    value = "application/json",
                                    disabled = false,
                                    description = new
                                    {
                                        content = "",
                                        type = "text/markdown",
                                        version = _options.Value.ApiVersion
                                    },
                                }
                            },
                            body = default(object)
                        },
                        response = new List<dynamic>(),
                        protocolProfileBehavior = new { }
                    };
                    folder.item.Add(item);
                }
                collection.item.Add(folder);
            }

            return Ok(collection);
        }
    }
}
