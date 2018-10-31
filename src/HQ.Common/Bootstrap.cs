// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace HQ.Common
{
	public class Bootstrap
	{
		static Bootstrap()
		{
			JsonConvert.DefaultSettings = () =>
			{
				var settings = new JsonSerializerSettings
				{
					Formatting = Formatting.None,
					TypeNameHandling = TypeNameHandling.None,
					NullValueHandling = NullValueHandling.Ignore,
					DefaultValueHandling = DefaultValueHandling.Ignore,
					ContractResolver = new CamelCasePropertyNamesContractResolver(),

					DateTimeZoneHandling = DateTimeZoneHandling.Utc,
					DateParseHandling = DateParseHandling.DateTimeOffset,
					DateFormatHandling = DateFormatHandling.IsoDateFormat,
					DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"
				};

				settings.Converters.Add(new StringEnumConverter());

				return settings;
			};
		}

		public static void EnsureInitialized()
		{
		}
	}
}