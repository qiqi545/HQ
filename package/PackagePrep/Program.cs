// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace PackagePrep
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0 || args.Length > 3 ||
                args[0].Equals("tokenize", StringComparison.OrdinalIgnoreCase) && args.Length != 3 ||
                args[0].Equals("nuspec", StringComparison.OrdinalIgnoreCase) && args.Length != 3)
            {
                Console.WriteLine("Usage: PackagePrep [tokenize|nuspec] [project-directory] [rootNamespace|devDependency]");
                Environment.Exit(1);
            }

            Console.WriteLine($"PackagePrep {string.Join(' ', args)}");
            Console.WriteLine();

            void ReplaceFileInArchive(ZipArchiveEntry entry, string content)
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
                var rootNamespace = args[2];

                Console.WriteLine("Tokenizing sources...");
                Console.WriteLine("Path: " + path);
                Console.WriteLine("Namespace: " + rootNamespace);

                foreach (var file in Directory.GetFiles(path, "*.pp", SearchOption.AllDirectories))
                    File.Delete(file);

                foreach (var file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
                {
                    rootNamespace = rootNamespace.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];

                    var text = File.ReadAllText(file, Encoding.UTF8);
                    text = text.Replace($"{rootNamespace}.", "$RootNamespace$.");

                    var sb = new StringBuilder();
                    sb.AppendLine(text);

                    var code = sb.ToString();
                    File.WriteAllText(file + ".pp", code, Encoding.UTF8);
                }
            }

            if (command.Equals("nuspec"))
            {
                var path = args[1];
                var dependencies = !bool.Parse(args[2]);

                Console.WriteLine("Correcting nuspecs...");
                Console.WriteLine($"Path: {path}");
                Console.WriteLine($"Package without dependencies: {dependencies.ToString().ToLowerInvariant()}");

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

                                var updated = before
                                    .Replace("exclude=\"Build,Analyzers\"", string.Empty)   // fix PackageReferences
                                    .Replace("buildAction=\"Content\"", string.Empty);      // fix file tags

                                if (!dependencies)
                                {
                                    var hasDependencies = Regex.Match(updated, "<dependencies>([\\s\\S]*?)</dependencies>");
                                    if (hasDependencies.Success)
                                    {
                                        Console.WriteLine("Removing dependencies...");
                                        Console.WriteLine(hasDependencies.Value);
                                        updated = updated.Replace(hasDependencies.Value, "<!-- dependencies were elided -->");
                                    }
                                }

                                if (before == updated)
                                {
                                    Console.WriteLine(" not modified.");
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine(updated);
                                }

                                ReplaceFileInArchive(entry, updated);

                                var after = ReadFileInArchive(entry);

                                if (after != updated)
                                {
                                    throw new IOException("The update process did not change the .nuspec.");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
