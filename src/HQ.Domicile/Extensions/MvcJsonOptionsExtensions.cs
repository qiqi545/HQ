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
