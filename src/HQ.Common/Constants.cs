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

namespace HQ.Common
{
    public static class Constants
    {
        public static class ConnectionSlots
        {
            public const string Identity = "Identity";
            public const string Default = "Default";
        }

        public static class Sql
        {
            public const char ParentAlias = 'r';
            public const char ChildAlias = 'c';
            public const string SetSuffix = "_set";
        }

        public static class ContextKeys
        {
            public const string DynamicViewLocation = "DynamicViewLocation";
            public const string JsonMultiCase = "JsonMultiCase";
            public const string JsonTrim = "JsonTrim";
            public const string JsonPrettyPrint = "JsonPrettyPrint";
            public const string CacheArgument = "cache";
            public const string CacheKeyArgument = "cacheKey";
            public const string Tenant = "tenant";
            public const string Version = "version";
        }

        public static class PlatformRoutes
        {
            public const string RouteDebug = "route_debug";
        }

        public static class EnvironmentVariables
        {
            public const string Name = "ASPNETCORE_ENVIRONMENT";
        }

        public static class Identity
        {
            public const string DefaultCollection = "AspNetIdentity";
        }

        public static class Schema
        {
            public const string DefaultNamespace = "MyNamespace";
        }

        public static class Security
        {
            public static class Policies
            {
                public const string SuperUserOnly = "SuperUserOnly";
                public const string ManageTenants = "ManageTenants";
                public const string ManageUsers = "ManageUsers";
                public const string ManageRoles = "ManageRoles";
            }
        }

        public static class Versioning
        {
            public const string DefaultVersion = "1.0";
            public const string VersionHeader = "X-Api-Version";
            public const string VersionParameter = "api-version";
            public const string VersionPathPrefix = "v";
            public const string UserVersionClaim = "version";
        }

        public static class MultiTenancy
        {
            public const string DefaultTenantName = "defaultTenant";
            public const string TenantHeader = "X-Api-Tenant";
        }

        public static class QueryStrings
        {
            public const string MultiCase = "case";
            public const string Envelope = "envelope";
            public const string Trim = "trim";
            public const string PrettyPrint = "prettyPrint";
        }

        public static class Loggers
        {
            public const string Formatters = "Formatters";
        }

        public static class Categories
        {
            public const string Metrics = "Metrics";
        }

        public static class HttpHeaders
        {
            public const string ContentType = "Content-Type";
            public const string ETag = "ETag";
            public const string IfMatch = "If-Match";
            public const string IfNoneMatch = "If-None-Match";
            public const string LastModified = "Last-Modified";
            public const string IfModifiedSince = "If-Modified-Since";
            public const string IfUnmodifiedSince = "If-Unmodified-Since";
            public const string Link = "Link";
            public const string Location = "Location";

            public const string MethodOverride = "X-HTTP-Method-Override";
            public const string Action = "X-Action";
            public const string TotalCount = "X-Total-Count";
            public const string TotalPages = "X-Total-Pages";

            public const string TenantHeader = "X-Tenant";

            /// <summary>
            ///     See: https://www.w3.org/TR/server-timing/
            /// </summary>
            public const string ServerTiming = "Server-Timing";

            /// <summary>
            ///     See: https://www.w3.org/TR/server-timing/
            /// </summary>
            public const string TimingAllowOrigin = "Timing-Allow-Origin";
            
            /// <summary>
            ///     See: https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#75-standard-request-headers
            /// </summary>
            public const string Prefer = "Prefer";
        }

        public static class HttpVerbs
        {
            public const string Trace = "TRACE";
            public const string Options = "OPTIONS";
            public const string Head = "HEAD";
            public const string Get = "GET";
            public const string Post = "POST";
            public const string Put = "PUT";
            public const string Patch = "PATCH";
            public const string Delete = "DELETE";
        }

        public static class MediaTypes
        {
            public const string Json = "application/json";
            public const string JsonPatch = "application/json-patch+json";
            public const string JsonMergePatch = "application/merge-patch+json";
            public const string JsonSchema = "application/schema+json";
            public const string OpenApiJson = "application/vnd.oai.openapi+json";
            public const string JsonApi = "application/vnd.api+json";
            public const string HqSchema = "application/vnd.hq.schema+json";

            public const string Xml = "application/xml";
        }
    }
}
