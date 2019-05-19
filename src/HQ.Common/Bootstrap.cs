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
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace HQ.Common
{
    public class Bootstrap
    {
        public static void EnsureInitialized()
        {
            try
            {
                SetDefaultJsonSettings();
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Bootstrapper failed unexpectedly: {0}", e);
            }
        }

        public static void SetDefaultJsonSettings()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None,
                    
                    ContractResolver = new ContractResolver(),
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,

                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateParseHandling = DateParseHandling.DateTimeOffset,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"
                };
                settings.Converters.Add(new StringEnumConverter());

                return settings;
            };
        }

        public class ContractResolver : DefaultContractResolver
        {
            public static HashSet<Type> IgnoreTypes = new HashSet<Type>();

            public ContractResolver()
            {
                IgnoreTypes.Add(typeof(Assembly));
                IgnoreTypes.Add(typeof(Module));
                IgnoreTypes.Add(typeof(Type));
                IgnoreTypes.Add(typeof(MethodBase));
                IgnoreTypes.Add(typeof(MemberInfo));
                IgnoreTypes.Add(typeof(RuntimeMethodHandle));
                IgnoreTypes.Add(typeof(Delegate));
                IgnoreTypes.Add(typeof(IServiceProvider));

                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true,
                    OverrideSpecifiedNames = true
                };
            }
            
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (IgnoreTypes.Contains(property.PropertyType) || typeof(MulticastDelegate).IsAssignableFrom(property.PropertyType.BaseType))
                {
                    if(Debugger.IsAttached)
                        Debugger.Break();
                    Trace.TraceError($"Runtime avoided JSON serializing property '{property.PropertyType.Name} {property.DeclaringType.Name}.{property.PropertyName}' that would cause a circular reference.");
                    property.ShouldSerialize = i => false;
                    property.Ignored = true;
                }

                return property;
            }
        }
    }
}
