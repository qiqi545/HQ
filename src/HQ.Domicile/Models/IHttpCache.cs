// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Domicile.Models
{
	public interface IHttpCache
	{
		bool TryGetETag(string key, out string etag);
		bool TryGetLastModified(string key, out DateTimeOffset lastModified);
		void Save(string key, string etag);
		void Save(string key, DateTimeOffset lastModified);
	}
}