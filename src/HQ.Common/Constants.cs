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
            public const string DynamicViewLocation = nameof(DynamicViewLocation);
            public const string JsonMultiCase = nameof(JsonMultiCase);
            public const string JsonTrim = nameof(JsonTrim);
            public const string JsonPrettyPrint = nameof(JsonPrettyPrint);
            public const string CacheArgument = nameof(CacheArgument);
            public const string CacheKeyArgument = nameof(CacheKeyArgument);
            public const string Tenant = nameof(Tenant);
            public const string Application = nameof(Application);
            public const string Version = nameof(Version);
            public const string AnonymousUserId = nameof(AnonymousUserId);
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

        public static class Schemas
        {
            public const string DefaultNamespace = "MyNamespace";
        }

        public static class Security
        {
            public static class Policies
            {
                public const string SuperUserOnly = "SuperUserOnly";
                public const string ManageTenants = "ManageTenants";
                public const string ManageApplications = "ManageApplications";
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
            public const string ApplicationHeader = "X-Api-Application";
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
            public const string TraceParent = "traceparent";

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

            public const string PreferenceApplied = "Preference-Applied";
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

            public const string Markdown = "text/markdown";
            public const string PlainText = "text/plain";
            public const string Binary = "application/octet-stream";
            public const string Unspecified = Binary;
        }

        public static class Events
        {
            public const string HealthCheck = "health.check";
            public const string MetricsSample = "metrics.sample";
        }

        public static class Tokens
        {
            public const string NoSigningKeySet = "PRIVATE-KEY-REPLACE-ME";
            public const string NoEncryptionKeySet = "ENCRYPTION-KEY-REPLACE-ME";
            public const string DefaultPath = "/tokens";
        }

        public static class Cookies
        {
            public const string IdentityName = "auth";
            public const string AnonymousIdentityName = "auth-anon";
            public const string SignInPath = "/signin";
            public const string SignOutPath = "/signout";
            public const string ForbidPath = "/denied";
            public const string ReturnOperator = "return";
        }

        public static class Claims
        {
            public const string ApplicationName = "appName";
            public const string ApplicationId = "appId";
            public const string TenantId = "tenantId";
            public const string TenantName = "tenantName";
            public const string Permission = "userPermission";
            public const string Email = "userEmail";
            public const string Role = "userRole";
            public const string UserName = "userName";
            public const string UserId = "userId";
        }
    }
}
