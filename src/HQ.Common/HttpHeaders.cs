// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Common
{
	internal static class HttpHeaders
	{
		public const string ContentType = "Content-Type";
		public const string IfModifiedSince = "If-Modified-Since";
		public const string IfNoneMatch = "If-None-Match";
		public const string IfUnmodifiedSince = "If-Unmodified-Since";
		public const string LastModified = "Last-Modified";
		public const string ETag = "ETag";

		public const string MethodOverride = "X-HTTP-Method-Override";
		public const string Action = "X-Action";
	}
}