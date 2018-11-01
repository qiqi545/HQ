// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HQ.Domicile.Extensions
{
	public static class MvcJsonOptionsExtensions
	{
		public static void Apply(this MvcJsonOptions options, JsonSerializerSettings right)
		{
			options.SerializerSettings.CheckAdditionalContent = right.CheckAdditionalContent;
			options.SerializerSettings.ConstructorHandling = right.ConstructorHandling;
			options.SerializerSettings.Context = right.Context;
			options.SerializerSettings.ContractResolver = right.ContractResolver;
			options.SerializerSettings.Converters = right.Converters;
			options.SerializerSettings.Culture = right.Culture;
			options.SerializerSettings.DateFormatHandling = right.DateFormatHandling;
			options.SerializerSettings.DateFormatString = right.DateFormatString;
			options.SerializerSettings.DateParseHandling = right.DateParseHandling;
			options.SerializerSettings.DateTimeZoneHandling = right.DateTimeZoneHandling;
			options.SerializerSettings.DefaultValueHandling = right.DefaultValueHandling;
			options.SerializerSettings.EqualityComparer = right.EqualityComparer;
			options.SerializerSettings.Error = right.Error;
			options.SerializerSettings.FloatFormatHandling = right.FloatFormatHandling;
			options.SerializerSettings.FloatParseHandling = right.FloatParseHandling;
			options.SerializerSettings.Formatting = right.Formatting;
			options.SerializerSettings.MaxDepth = right.MaxDepth;
			options.SerializerSettings.MetadataPropertyHandling = right.MetadataPropertyHandling;
			options.SerializerSettings.MissingMemberHandling = right.MissingMemberHandling;
			options.SerializerSettings.NullValueHandling = right.NullValueHandling;
			options.SerializerSettings.ObjectCreationHandling = right.ObjectCreationHandling;
			options.SerializerSettings.PreserveReferencesHandling = right.PreserveReferencesHandling;
			options.SerializerSettings.ReferenceLoopHandling = right.ReferenceLoopHandling;
			options.SerializerSettings.ReferenceResolverProvider = right.ReferenceResolverProvider;
			options.SerializerSettings.SerializationBinder = right.SerializationBinder;
			options.SerializerSettings.StringEscapeHandling = right.StringEscapeHandling;
			options.SerializerSettings.TraceWriter = right.TraceWriter;
			options.SerializerSettings.TypeNameAssemblyFormatHandling = right.TypeNameAssemblyFormatHandling;
			options.SerializerSettings.TypeNameHandling = right.TypeNameHandling;
		}
	}
}