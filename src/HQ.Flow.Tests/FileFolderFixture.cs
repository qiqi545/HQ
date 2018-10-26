// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.IO;

namespace HQ.Flow.Tests
{
	public class FileFolderFixture : IDisposable
	{
		public readonly string Folder = $"FileFolder-{Guid.NewGuid()}";

		public FileFolderFixture()
		{
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);
			foreach (var file in Directory.GetFiles(Folder, "*.*"))
				File.Delete(file);
		}

		public void Dispose()
		{
			if (Directory.Exists(Folder))
			{
				foreach (var file in Directory.GetFiles(Folder, "*.*"))
					File.Delete(file);
				Directory.Delete(Folder);
			}
		}
	}
}