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
using System.Diagnostics;
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
                JsonConvert.DefaultSettings = () =>
                {
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None,

                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
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
            catch (Exception e)
            {
                Trace.TraceWarning("Bootstrapper failed unexpectedly: {0}", e);
            }
        }
    }
}
