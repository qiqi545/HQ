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

// ReSharper disable once CheckNamespace
namespace System.Net.Mime
{
	public static class MediaTypeNamesExt
	{
		public static class Application
		{
			public const string JsonPatch = "application/json-patch+json";
			public const string JsonMergePatch = "application/merge-patch+json";
			public const string GraphQl = "application/graphql";
		}

		public static class Text
		{
			public const string Markdown = "text/markdown";
		}

		/*
		    public const string Json = "application/json";			
			public const string JsonSchema = "application/schema+json";
			public const string OpenApiJson = "application/vnd.oai.openapi+json";
			public const string JsonApi = "application/vnd.api+json";
			public const string HqSchema = "application/vnd.hq.schema+json";						
			public const string PlainText = "text/plain";
			public const string Binary = "application/octet-stream";
			public const string Unspecified = Binary;
         */
	}
}