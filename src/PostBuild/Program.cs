﻿#region LICENSE

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
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PostBuild
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("PostBuild");
			Console.WriteLine("---------");
			Console.WriteLine(string.Join(' ', args));

			void ReplaceFileInArchive(ZipArchive archive, ZipArchiveEntry entry, FileInfo file, string content)
			{
				using (var stream = entry.Open())
				{
					stream.SetLength(content.Length);
					using (var sw = new StreamWriter(stream))
						sw.Write(content);
				}
			}

			string ReadFileInArchive(ZipArchiveEntry entry)
			{
				using (var sr = new StreamReader(entry.Open()))
				{
					return sr.ReadToEnd();
				}
			}

			var command = args[0];

			if (command.Equals("tokenize", StringComparison.OrdinalIgnoreCase))
			{
				var path = args[1];

				Console.WriteLine("Tokenizing sources...");
				Console.WriteLine("Path: " + path);

				foreach (var file in Directory.GetFiles(path, "*.pp", SearchOption.AllDirectories))
					File.Delete(file);

				foreach (var file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
				{
					var text = File.ReadAllText(file, Encoding.UTF8);

					text = text.Replace("HQ.", "$RootNamespace$.");

					File.WriteAllText(file + ".pp", text, Encoding.UTF8);
				}
			}

			if (command.Equals("nuspec"))
			{
				var path = args[1];

				Console.WriteLine("Correcting nuspecs...");
				Console.WriteLine("Path: " + path);

				foreach (var package in Directory.GetFiles(path, "*.nupkg", SearchOption.AllDirectories))
				{
					Console.WriteLine("Found package: " + package);

					using (var archive = ZipFile.Open(package, ZipArchiveMode.Update))
					{
						for (int i = archive.Entries.Count - 1; i >= 0; i--)
						{
							var entry = archive.Entries[i];
							var file = new FileInfo(entry.FullName);
							if (file.Extension == ".nuspec")
							{
								var before = ReadFileInArchive(entry);

								Console.Write("Found .nuspec: " + file.Name + "...");

								var updated = before.Replace("buildAction=\"Content\"", string.Empty);

								if (before == updated)
								{
									Console.WriteLine(" not modified.");
								}
								else
								{
									Console.WriteLine();
									Console.WriteLine(updated);
								}

								ReplaceFileInArchive(archive, entry, file, updated);

								var after = ReadFileInArchive(entry);

								if (after != updated)
								{
									throw new FileLoadException("The update process did change the .nuspec.");
								}
							}
						}
					}
				}
			}
		}
	}
}
