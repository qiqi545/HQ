using System.ComponentModel;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Platform.Operations.Configuration;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace HQ.Platform.Operations.Controllers
{
	[Route("configuration")]
	[DynamicAuthorize(typeof(ConfigurationApiOptions))]
	[DynamicController]
	[MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
    [MetaDescription("Manages configuration items.")]
    [DisplayName("Configuration")]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class ConfigurationController : DataController
    {
        private readonly IConfigurationRoot _root;

        public ConfigurationController(IConfigurationRoot root)
        {
            _root = root;
        }

        [HttpGet("")]
        [HttpGet("{section?}")]
        public IActionResult Get([FromRoute] string section = null)
        {
            if (string.IsNullOrWhiteSpace(section))
                return NotAcceptableError(ErrorEvents.UnsafeRequest, "You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");

            var config = _root.GetSection(section.Replace("/", ":"));
            if (config == null)
                return NotFound();

            var content = Serialize(config).ToString();
            return Content(content, Constants.MediaTypes.Json);
        }

        public JToken Serialize(IConfiguration config)
        {
            var instance = new JObject();
            foreach (var child in config.GetChildren())
                instance.Add(child.Key, Serialize(child));

            return !instance.HasValues && config is IConfigurationSection section
                ? (JToken) new JValue(section.Value)
                : instance;
        }
    }
}
