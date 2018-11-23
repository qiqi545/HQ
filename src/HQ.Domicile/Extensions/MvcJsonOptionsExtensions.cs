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

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HQ.Domicile.Extensions
{
    public static class MvcJsonOptionsExtensions
    {
        public static void Apply(this MvcJsonOptions options, JsonSerializerSettings settings)
        {
            options.SerializerSettings.CheckAdditionalContent = settings.CheckAdditionalContent;
            options.SerializerSettings.ConstructorHandling = settings.ConstructorHandling;
            options.SerializerSettings.Context = settings.Context;
            options.SerializerSettings.ContractResolver = settings.ContractResolver;
            options.SerializerSettings.Converters = settings.Converters;
            options.SerializerSettings.Culture = settings.Culture;
            options.SerializerSettings.DateFormatHandling = settings.DateFormatHandling;
            options.SerializerSettings.DateFormatString = settings.DateFormatString;
            options.SerializerSettings.DateParseHandling = settings.DateParseHandling;
            options.SerializerSettings.DateTimeZoneHandling = settings.DateTimeZoneHandling;
            options.SerializerSettings.DefaultValueHandling = settings.DefaultValueHandling;
            options.SerializerSettings.EqualityComparer = settings.EqualityComparer;
            options.SerializerSettings.Error = settings.Error;
            options.SerializerSettings.FloatFormatHandling = settings.FloatFormatHandling;
            options.SerializerSettings.FloatParseHandling = settings.FloatParseHandling;
            options.SerializerSettings.Formatting = settings.Formatting;
            options.SerializerSettings.MaxDepth = settings.MaxDepth;
            options.SerializerSettings.MetadataPropertyHandling = settings.MetadataPropertyHandling;
            options.SerializerSettings.MissingMemberHandling = settings.MissingMemberHandling;
            options.SerializerSettings.NullValueHandling = settings.NullValueHandling;
            options.SerializerSettings.ObjectCreationHandling = settings.ObjectCreationHandling;
            options.SerializerSettings.PreserveReferencesHandling = settings.PreserveReferencesHandling;
            options.SerializerSettings.ReferenceLoopHandling = settings.ReferenceLoopHandling;
            options.SerializerSettings.ReferenceResolverProvider = settings.ReferenceResolverProvider;
            options.SerializerSettings.SerializationBinder = settings.SerializationBinder;
            options.SerializerSettings.StringEscapeHandling = settings.StringEscapeHandling;
            options.SerializerSettings.TraceWriter = settings.TraceWriter;
            options.SerializerSettings.TypeNameAssemblyFormatHandling = settings.TypeNameAssemblyFormatHandling;
            options.SerializerSettings.TypeNameHandling = settings.TypeNameHandling;
        }
    }
}
