using System;
using System.ComponentModel;
using System.Reflection;
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
using Newtonsoft.Json;
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
			return SubKeyIsMissing(section, out var notAcceptable) ? notAcceptable : GetSerialized(section);
		}

		private IActionResult GetSerialized(string section)
		{
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
				return NotFoundError(ErrorEvents.InvalidParameter, $"Configuration sub-key path '{section}' not found.");

			var prototype = _typeResolver.FindFirstByName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"No configuration type found with name '{type}'.");

			var optionsType = typeof(ISaveOptions<>).MakeGenericType(prototype);
			var saveOptions = _serviceProvider.GetService(optionsType);
			if (saveOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"Could not resolve IOptions<{type}>");

			var valueMethod = optionsType.GetProperty(nameof(ISaveOptions<object>.Value));
			var trySaveMethod = optionsType.GetMethod(nameof(ISaveOptions<object>.TrySave));
			if (trySaveMethod == null || valueMethod == null)
				return InternalServerError(ErrorEvents.PlatformError, $"Unexpected error: ISaveOptions<{type}> methods failed to resolve.");

			SetSerialized(section, config, prototype, trySaveMethod, saveOptions, valueMethod);

			return GetSerialized(section);
		}

		private void SetSerialized(string section, IConfigurationSection config, Type prototype, MethodInfo trySaveMethod,
			object saveOptions, PropertyInfo valueMethod)
		{
			var content = Serialize(config).ToString();

			var result = JsonConvert.DeserializeObject(content, prototype);

			trySaveMethod.Invoke(saveOptions, new object[]
			{
				section, new Action(() =>
				{
					var target = valueMethod.GetValue(saveOptions);

					var writer = WriteAccessor.Create(prototype, out var members);
					var reader = ReadAccessor.Create(prototype);

					foreach (var member in members)
					{
						if (member.CanWrite && member.CanRead &&
						    reader.TryGetValue(result, member.Name, out var value))
						{
							writer.TrySetValue(target, member.Name, value);
						}
					}
				})
			});
		}

		private bool SubKeyIsMissing(string section, out IActionResult result)
		{
			if (string.IsNullOrWhiteSpace(section))
			{
				result = NotAcceptableError(ErrorEvents.UnsafeRequest, "You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");
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
