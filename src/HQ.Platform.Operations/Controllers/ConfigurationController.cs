using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Extensions.Options;
using HQ.Platform.Operations.Configuration;
using HQ.Platform.Operations.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Morcatko.AspNetCore.JsonMergePatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TypeKitchen;

namespace HQ.Platform.Operations.Controllers
{
	[Route("configuration")]
	[DynamicAuthorize(typeof(ConfigurationApiOptions))]
	[DynamicController(typeof(ConfigurationApiOptions))]
	[MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
	[MetaDescription("Manages configuration items.")]
	[DisplayName("Configuration")]
	[ApiExplorerSettings(IgnoreApi = false)]
	public class ConfigurationController : DataController, IDynamicComponentEnabled<ConfigurationComponent>
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

		[FeatureSelector]
		[HttpGet("")]
		[HttpGet("{section?}"), MustHaveQueryParameters("type")]
		public IActionResult Get([FromQuery] string type, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest, "You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");

			var prototype = _typeResolver.FindFirstByName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"No configuration type found with name '{type}'.");

			return GetSerialized(prototype, section);
		}

		[FeatureSelector]
		[HttpPatch("")]
		[HttpPatch("{section?}"), MustHaveQueryParameters("type")]
		[Consumes(Constants.MediaTypes.JsonPatch)]
		public IActionResult Patch([FromQuery] string type, [FromBody] JsonPatchDocument patch, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest, "You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");

			var prototype = _typeResolver.FindFirstByName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"No configuration type found with name '{type}'.");

			var config = _root.GetSection(section.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter, $"Configuration sub-key path '{section}' not found.");
			
			var model = Activator.CreateInstance(prototype);
			config.FastBind(model);
			patch.ApplyTo(model);

			return Set(type, model, section);
		}

		[FeatureSelector]
		[HttpPatch("")]
		[HttpPatch("{section?}"), MustHaveQueryParameters("type")]
		[Consumes(Constants.MediaTypes.JsonMergePatch)]
		public IActionResult Patch([FromQuery] string type, [FromBody] object patch, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest,
					"You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");

			var prototype = _typeResolver.FindFirstByName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter, $"No configuration type found with name '{type}'.");

			var config = _root.GetSection(section.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter, $"Configuration sub-key path '{section}' not found.");

			var buildMethod = typeof(JsonMergePatchDocument).GetMethod(nameof(JsonMergePatchDocument.Build), new[] { typeof(JObject) })?.MakeGenericMethod(prototype);
			var patchModel = buildMethod?.Invoke(null, new[] {patch});

			var model = Activator.CreateInstance(prototype);
			config.FastBind(model);

			var patchType = typeof(JsonMergePatchDocument<>).MakeGenericType(prototype);
			var patchMethods = patchType.GetTypeInfo().DeclaredMethods;
			var patchMethod = patchMethods.Single(x => x.Name == "ApplyTo" && !x.IsGenericMethod);
			patchMethod?.Invoke(patchModel, new[] { model });

			return Set(type, model, section);
		}

		[FeatureSelector]
		[HttpPut("")]
		[HttpPut("{section?}"), MustHaveQueryParameters("type")]
		public IActionResult Set([FromQuery] string type, [FromBody] object model, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest, "You must specify a known configuration sub-key, to avoid exposing sensitive root-level data.");

			if (model == null)
				return NotAcceptableError(ErrorEvents.InvalidRequest, "Missing configuration body.");

			var config = _root.GetSection(section.Replace("/", ":"));
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
			if (trySaveMethod == null || valueProperty == null)
				return InternalServerError(ErrorEvents.PlatformError, $"Unexpected error: IOptions<{type}> methods failed to resolve.");

			object result;
			if (model is JObject)
			{
				var json = model.ToString();
				result = JsonConvert.DeserializeObject(json, prototype);
			}
			else
				result = model;

			return TrySave(section, result, trySaveMethod, saveOptions, prototype, valueProperty);
		}

		private IActionResult TrySave(string section, object result, MethodBase trySaveMethod, object saveOptions, Type prototype, PropertyInfo valueProperty)
		{
			try
			{
				result.Validate(_serviceProvider);
			}
			catch (ValidationException e)
			{
				return UnprocessableEntityError(ErrorEvents.ValidationFailed, e.ValidationResult.ErrorMessage);
			}

			var saved = trySaveMethod.Invoke(saveOptions, new object[]
			{
				section, new Action(() => { SaveOptions(prototype, saveOptions, valueProperty, result); })
			});

			if (saved is bool flag && !flag)
			{
				return NotModified();
			}

			return GetSerialized(prototype, section);
		}

		private IActionResult GetSerialized(Type type, string section)
		{
			var config = _root.GetSection(section?.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter, $"Configuration sub-key path '{section}' not found.");

			var template = Activator.CreateInstance(type);
			config.FastBind(template);

			return Ok(template);
		}

		private static void SaveOptions(Type type, object saveOptions, PropertyInfo valueProperty, object result)
		{
			var target = valueProperty.GetValue(saveOptions);
			var writer = WriteAccessor.Create(type, out var members);
			var reader = ReadAccessor.Create(type);

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
		}
	}
}
