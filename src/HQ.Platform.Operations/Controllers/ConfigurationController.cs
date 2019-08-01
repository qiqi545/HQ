using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Extensions.Options;
using HQ.Platform.Operations.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
		public IActionResult Set([FromQuery] string type, [FromBody] object model, [FromRoute] string section = null)
		{
			if (SubKeyIsMissing(section, out var notAcceptable))
				return notAcceptable;

			if (model == null)
				return NotAcceptableError(ErrorEvents.InvalidRequest, "Missing configuration body.");

			var config = _root.GetSection(section?.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter, $"Configuration sub-key path '{section}' not found.");

			var prototype = _typeResolver.FindFirstByName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"No configuration type found with name '{type}'.");

			var optionsType = typeof(IOptions<>).MakeGenericType(prototype);
			var saveOptionsType = typeof(ISaveOptions<>).MakeGenericType(prototype);
			var saveOptions = _serviceProvider.GetService(saveOptionsType);
			if (saveOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"Could not resolve IOptions<{type}> for saving");

			var validOptionsType = typeof(IValidOptionsMonitor<>).MakeGenericType(prototype);
			var validOptions = _serviceProvider.GetService(validOptionsType);
			if (validOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"Could not resolve IOptions<{type}> for validation");

			var valueProperty = optionsType.GetProperty(nameof(IOptions<object>.Value));
			var trySaveMethod = saveOptionsType.GetMethod(nameof(ISaveOptions<object>.TrySave), new [] { typeof(string), typeof(Action)});
			var getMethod = typeof(IOptionsMonitor<object>).GetMethod(nameof(IOptionsMonitor<object>.Get));
			if (trySaveMethod == null || valueProperty == null || getMethod == null)
				return InternalServerError(ErrorEvents.PlatformError, $"Unexpected error: IOptions<{type}> methods failed to resolve.");

			try
			{
				getMethod.Invoke(validOptions, new object[] { Options.DefaultName });
			}
			catch (ValidationException e)
			{
				return UnprocessableEntityError(ErrorEvents.ValidationFailed, e.ValidationResult.ErrorMessage);
			}

			SetSerialized(section, config, prototype, trySaveMethod, saveOptions, model, valueProperty);

			return GetSerialized(section);
		}

		private static void SetSerialized(string section, IConfiguration config, Type prototype, MethodBase trySaveMethod,
			object saveOptions, object model, PropertyInfo valueMethod)
		{
			var json = model.ToString();
			var result = JsonConvert.DeserializeObject(json, prototype);

			trySaveMethod.Invoke(saveOptions, new object[]
			{
				section, new Action(() =>
				{
					var target = valueMethod.GetValue(saveOptions);
					var writer = WriteAccessor.Create(prototype, out var members);
					var reader = ReadAccessor.Create(prototype);

					foreach (var member in members)
					{
						if (member.MemberType == AccessorMemberType.Property &&
							member.CanWrite &&
							member.CanRead &&
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
