#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using HQ.Common.AspNetCore;
using HQ.Common.AspNetCore.Mvc;
using HQ.Common.AspNetCore.MergePatch.Builders;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Attributes;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Extensions.Options;
using HQ.Platform.Operations.Configuration;
using HQ.Platform.Operations.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TypeKitchen;
using TypeKitchen.Creation;

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
		private readonly IServiceProvider _serviceProvider;
		private readonly IEnumerable<ICustomConfigurationBinder> _customBinders;
		private readonly ConfigurationService _service;
		private readonly ITypeResolver _typeResolver;

		public ConfigurationController(IConfigurationRoot root, ITypeResolver typeResolver, IServiceProvider serviceProvider, IEnumerable<ICustomConfigurationBinder> customBinders, ConfigurationService service)
		{
			_root = root;
			_typeResolver = typeResolver;
			_serviceProvider = serviceProvider;
			_customBinders = customBinders;
			_service = service;
		}

		[FeatureSelector]
		[HttpGet("")]
		[HttpGet("{section?}")]
		[MustHaveQueryParameters("type")]
		public IActionResult Get([FromQuery] string type, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest,
					"You must specify a known configuration sub-section, to avoid exposing sensitive root-level data.");

			var prototype = ResolvePrototypeName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"No configuration type found with name '{type}'.");

			return GetSerialized(prototype, section);
		}


		[FeatureSelector]
		[HttpPatch("")]
		[HttpPatch("{section?}")]
		[MustHaveQueryParameters("type")]
		[Consumes(MediaTypeNames.Application.JsonPatch)]
		public IActionResult Patch([FromQuery] string type, [FromBody] JsonPatchDocument patch,
			[FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest,
					"You must specify a known configuration sub-section, to avoid exposing sensitive root-level data.");

			var prototype = ResolvePrototypeName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"No configuration type found with name '{type}'.");

			var config = _root.GetSection(section.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter,
					$"Configuration sub-section path '{section}' not found.");

			var model = Instancing.CreateInstance(prototype);
			config.FastBind(model, _customBinders);
			patch.ApplyTo(model);

			return Put(type, model, section);
		}

		[FeatureSelector]
		[HttpPatch("")]
		[HttpPatch("{section?}")]
		[MustHaveQueryParameters("type")]
		[Consumes(MediaTypeNames.Application.JsonMergePatch)]
		public IActionResult Patch([FromQuery] string type, [FromBody] object patch, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest,
					"You must specify a known configuration sub-section, to avoid exposing sensitive root-level data.");

			var prototype = ResolvePrototypeName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"No configuration type found with name '{type}'.");

			var config = _root.GetSection(section.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter,
					$"Configuration sub-section path '{section}' not found.");

			if(config.Value == null)
				return Put(type, patch, section);

			var original = Instancing.CreateInstance(prototype);
			config.FastBind(original, _customBinders);

			var diff = PatchBuilder.Build(original, patch);
			diff.ApplyTo(original);

			return Put(type, patch, section);
		}

		[FeatureSelector]
		[HttpPut("")]
		[HttpPut("{section?}")]
		[MustHaveQueryParameters("type")]
		public IActionResult Put([FromQuery] string type, [FromBody] object model, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest,
					"You must specify a known configuration sub-section, to avoid exposing sensitive root-level data.");

			if (model == null)
				return NotAcceptableError(ErrorEvents.InvalidRequest, "Missing configuration body.");

			var config = _root.GetSection(section.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter,
					$"Configuration sub-section path '{section}' not found.");

			var prototype = ResolvePrototypeName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"No configuration type found with name '{type}'.");

			var optionsType = typeof(IOptions<>).MakeGenericType(prototype);
			var saveOptionsType = typeof(ISaveOptions<>).MakeGenericType(prototype);
			var saveOptions = _serviceProvider.GetService(saveOptionsType);
			if (saveOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"Could not resolve IOptions<{type}> for saving");

			var validOptionsType = typeof(IValidOptionsMonitor<>).MakeGenericType(prototype);
			var validOptions = _serviceProvider.GetService(validOptionsType);
			if (validOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"Could not resolve IOptions<{type}> for validation");

			var valueProperty = optionsType.GetProperty(nameof(IOptions<object>.Value));
			if(valueProperty == null)
				return InternalServerError(ErrorEvents.PlatformError, $"Unexpected error: IOptions<{type}> methods failed to resolve.");

			var trySaveMethod = saveOptionsType.GetMethod(nameof(ISaveOptions<object>.TrySave), new[] {typeof(string), typeof(Action)});
			if (trySaveMethod == null)
				return InternalServerError(ErrorEvents.PlatformError, $"Unexpected error: IOptions<{type}> methods failed to resolve.");

			var tryAddMethod = saveOptionsType.GetMethod(nameof(ISaveOptions<object>.TryAdd), new[] {typeof(string), typeof(Action)});
			if (tryAddMethod == null)
				return InternalServerError(ErrorEvents.PlatformError, $"Unexpected error: IOptions<{type}> methods failed to resolve.");

			if (model is JObject)
			{
				var json = model.ToString();
				model = JsonConvert.DeserializeObject(json, prototype);
			}
			
			var result = TryUpsert(section, model, trySaveMethod, tryAddMethod, saveOptions, prototype, valueProperty);
			return result;
		}

		[FeatureSelector]
		[HttpDelete("")]
		[HttpDelete("{section?}")]
		public IActionResult Delete([FromQuery] string type, [FromRoute] string section = null)
		{
			if (string.IsNullOrWhiteSpace(section))
				return NotAcceptableError(ErrorEvents.UnsafeRequest,
					"You must specify a known configuration sub-section, to avoid exposing sensitive root-level data.");

			var config = _root.GetSection(section.Replace("/", ":"));
			if (config == null)
				return NotFoundError(ErrorEvents.InvalidParameter,
					$"Configuration sub-section path '{section}' not found.");

			var prototype = ResolvePrototypeName(type);
			if (prototype == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"No configuration type found with name '{type}'.");

			var optionsType = typeof(IOptions<>).MakeGenericType(prototype);
			var saveOptionsType = typeof(ISaveOptions<>).MakeGenericType(prototype);
			var saveOptions = _serviceProvider.GetService(saveOptionsType);
			if (saveOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"Could not resolve IOptions<{type}> for saving");

			var validOptionsType = typeof(IValidOptionsMonitor<>).MakeGenericType(prototype);
			var validOptions = _serviceProvider.GetService(validOptionsType);
			if (validOptions == null)
				return NotAcceptableError(ErrorEvents.InvalidParameter,
					$"Could not resolve IOptions<{type}> for validation");

			var valueProperty = optionsType.GetProperty(nameof(IOptions<object>.Value));

			var tryDeleteMethod = saveOptionsType.GetMethod(nameof(ISaveOptions<object>.TryDelete), new[] {typeof(string)});
			if (tryDeleteMethod == null || valueProperty == null)
				return InternalServerError(ErrorEvents.PlatformError, 
					$"Unexpected error: IOptions<{type}> methods failed to resolve.");

			var deleted = (DeleteOptionsResult) tryDeleteMethod.Invoke(saveOptions, new object[] { section });
			
			return deleted switch
			{
				DeleteOptionsResult.NotFound => NotFound(),
				DeleteOptionsResult.NoContent => NoContent(),
				DeleteOptionsResult.Gone => Gone(),
				DeleteOptionsResult.InternalServerError => InternalServerError(ErrorEvents.PlatformError, $"Could not delete section {section}"),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		private IActionResult TryUpsert(string section, object model, MethodBase trySaveMethod, MethodBase tryAddMethod, object saveOptions, Type prototype, PropertyInfo valueProperty)
		{
			try
			{
				model.Validate(_serviceProvider);
			}
			catch (ValidationException e)
			{
				return UnprocessableEntityError(ErrorEvents.ValidationFailed, e.ValidationResult.ErrorMessage);
			}

			var saveResult = trySaveMethod.Invoke(saveOptions, new object[]
			{
				section, new Action(() =>
				{
					SaveOptions(prototype, saveOptions, valueProperty, model);
				})
			});

			if (saveResult is SaveOptionsResult save)
			{
				switch (save)
				{
					case SaveOptionsResult.NotFound:
					{
						var addResult = (bool) tryAddMethod.Invoke(saveOptions, new object[]
						{
							section, new Action(() =>
							{
								SaveOptions(prototype, saveOptions, valueProperty, model);
							})
						});
						if (!addResult)
							return InternalServerError(ErrorEvents.PlatformError,
								$"Could not add existing configuration to section '{section}'");

						break;
					}
					case SaveOptionsResult.NotModified:
						return NotModified();
				}
			}

			var serialized = GetSerialized(prototype, section);

			return serialized;
		}

		private IActionResult GetSerialized(Type type, string section)
		{
			var template = _service.Get(type, section);

			return template == null
				? NotFoundError(ErrorEvents.InvalidParameter, $"Configuration sub-section path '{section}' not found.")
				: Ok(template);
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
		
		private Type ResolvePrototypeName(string type)
		{
			return _typeResolver.FindByFullName(type) ?? _typeResolver.FindFirstByName(type);
		}
	}
}