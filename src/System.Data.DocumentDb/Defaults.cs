// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace System.Data.DocumentDb
{
    internal static class Defaults
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,

            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"
        };

        public static readonly Dictionary<string, Type> Metadata = new Dictionary<string, Type>
        {
            // UseTypeDiscrimination:
            {"DocumentType", typeof(string)},

            // Azure:
            {"id", typeof(string)},
            {"_rid", typeof(string)},
            {"_self", typeof(string)},
            {"_etag", typeof(string)},
            {"_attachments", typeof(string)},
            {"_ts", typeof(long)}
        };
    }
}
