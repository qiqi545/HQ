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
            public const char ParentAlias = 'p';
            public const char ChildAlias = 'c';
            public const string SetSuffix = "_set";
        }

        public static class DocumentDb
        {
            public const string DefaultCollection = "Default";
        }

        public static class ContextKeys
        {
            public const string DynamicViewLocation = "DynamicViewLocation";
            public const string JsonMultiCase = "JsonMultiCase";
        }

        public static class PlatformRoutes
        {
            public const string RouteDebug = "route_debug";
        }

        public static class PlatformHeaders
        {

        }

        public static class QueryStrings
        {
            public const string MultiCase = "case";
        }

        public static class Loggers
        {
            public const string Formatters = "Formatters";
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

            public const string MethodOverride = "X-HTTP-Method-Override";
            public const string Action = "X-Action";

            /// <summary>
            ///     See: https://www.w3.org/TR/server-timing/
            /// </summary>
            public const string ServiceTiming = "Server-Timing";
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
        }

        public static class ClaimTypes
        {
            public const string AccountId = "app.account_id";
            public const string AccountName = "app.account_name";
            public const string ApplicationId = "app.app_id";
            public const string ApplicationName = "app.app_name";
            public const string UserId = "app.user_id";
            public const string UserName = "app.user_name";
            public const string Role = "app.role";
            public const string Permission = "app.permission";
        }

        public static class ClaimValues
        {
            public const string SuperUser = "superuser";
            public const string ManageUsers = "manage_users";
            public const string ManageRoles = "manage_roles";
        }
    }
}
