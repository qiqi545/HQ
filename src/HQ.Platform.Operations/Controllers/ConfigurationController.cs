using System;
using System.ComponentModel;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Extensions.Options;
using HQ.Platform.Operations.Configuration;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using TypeKitchen;

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
        private readonly ITypeResolver _typeResolver;
        private readonly IServiceProvider _serviceProvider;

        public ConfigurationController(IConfigurationRoot root, ITypeResolver typeResolver, IServiceProvider serviceProvider)
        {
	        _root = root;
	        _typeResolver = typeResolver;
	        _serviceProvider = serviceProvider;
        }

        [HttpGet("")]
        [HttpGet("{section?}")]
        public IActionResult Get([FromRoute] string section = null)
        {
			if (SubKeyIsMissing(section, out var notAcceptable))
				return notAcceptable;

			var config = _root.GetSection(section?.Replace("/", ":"));
            if (config == null)
                return NotFound();

            var content = Serialize(config).ToString();
            return Content(content, Constants.MediaTypes.Json);
        }

        [HttpPut("")]
        [HttpPut("{section?}"), MustHaveQueryParameters("type")]
        public IActionResult Set([FromQuery] string type, [FromRoute] string section = null)
        {
			if (SubKeyIsMissing(section, out var notAcceptable))
				return notAcceptable;

			var config = _root.GetSection(section?.Replace("/", ":"));
			if (config == null)
				return NotFound();

			var prototype = _typeResolver.FindFirstByName(type);
			var optionsType = typeof(ISaveOptions<>).MakeGenericType(prototype);
			var saveOptions = _serviceProvider.GetRequiredService(optionsType);

			var content = Serialize(config).ToString();
	        return Content(content, Constants.MediaTypes.Json);
        }

        private bool SubKeyIsMissing(string section, out IActionResult result)
        {
	        if (string.IsNullOrWhiteSpace(section))
	        {
		        result = NotAcceptableError(ErrorEvents.UnsafeRequest, 
			        "You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");
		        return true;
	        }
	        result = null;
	        return false;
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
