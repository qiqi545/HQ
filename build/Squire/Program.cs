// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Squire
{
    class Program
    {
	    private static void Main(string[] args)
        {
            Masthead();

            if (args.Length != 5)
            {
                WL(@"Usage: Squire.exe $(MSBuildProjectDirectory) $(Configuration) $(TargetFramework) $(SourceName) $(SourceVersion)");
                Environment.Exit(1);
            }

            var currentDir = Environment.CurrentDirectory;
            WL("Working directory: " + currentDir);

            var projectDir = args[0];
            var configuration = args[1];
            var targetFramework = args[2];
            var sourceName = args[3];
            var sourceVersion = args[4];

            // $(ProjectDir)obj\$(Configuration)\$(TargetFramework)\NuGet\
            var locations = new HashSet<string>();
            var startIn = Path.Combine(projectDir, "obj", configuration, targetFramework, "NuGet");
            if (!Directory.Exists(startIn))
            {
                WL("No contentFiles found. You must add <IncludeAssets>contentFiles</IncludeAssets> to your <PackageReference> tag, or you have no contentFiles to process.");
                return;
            }

            foreach (var folder in Directory.GetDirectories(startIn, "*.*", SearchOption.AllDirectories))
            {
                // (SourceChecksum)\$(SourceName)\$(SourceVersion)
                var packageId = Path.Combine(sourceName, sourceVersion);
                if (folder.Contains(packageId))
                {
                    var checksumDir = folder.Substring(0, folder.IndexOf(packageId, StringComparison.Ordinal));
                    var location = Path.Combine(Path.GetDirectoryName(checksumDir), packageId);
                    locations.Add(location);
                }
            }

            if (locations.Count == 0)
            {
                WL("No contentFiles found. You must add <IncludeAssets>contentFiles</IncludeAssets> to your <PackageReference> tag, or you have no <contentFiles/> to manage.");
                Environment.Exit(2);
            }

            if (locations.Count > 1)
            {
                WL("Source was resolved in multiple locations. Please clean your /obj/ folder and try again.");
                Environment.Exit(3);
            }

            W($"Managing {sourceName}.{sourceVersion}...");

            var statement = false;
            var sourceDir = locations.Single();
            foreach (var sourceFile in Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories))
            {
                var sourcePath = Path.GetRelativePath(sourceDir, sourceFile);
                var destinationFile = Path.Combine(projectDir, sourcePath);
                var hashFile = Path.Combine(currentDir, "bin", "md5", sourcePath + ".md5");

                var sourceInfo = new FileInfo(sourceFile);
                var destinationInfo = new FileInfo(destinationFile);

                if (File.Exists(destinationFile))
                {
                    var sourceHash = GetChecksum(sourceInfo);
                    var destinationHash = File.Exists(hashFile)
                        ? File.ReadAllBytes(hashFile)
                        : GetChecksum(destinationInfo);
                    
                    if (sourceHash.Length != destinationHash.Length)
                    {
                        FilesDiffer(destinationInfo, sourceInfo, destinationFile, sourceFile, hashFile, sourceHash, sourcePath);

                        if (!statement)
                        {
                            WriteStartWork(locations);
                            statement = true;
                        }

                        continue;
                    }

                    var filesDiffer = false;
                    for (var i = 0; i < sourceHash.Length; i++)
                    {
                        if (sourceHash[i] == destinationHash[i])
                            continue;
                        filesDiffer = true;
                        break;
                    }

                    if (filesDiffer)
                    {
                        FilesDiffer(destinationInfo, sourceInfo, destinationFile, sourceFile, hashFile, sourceHash, sourcePath);

                        if (!statement)
                        {
                            WriteStartWork(locations);
                            statement = true;
                        }
                    }
                }
                else
                {
                    var sourceHash = GetChecksum(sourceInfo);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    File.Copy(sourceFile, destinationFile, true);

                    Directory.CreateDirectory(Path.GetDirectoryName(hashFile));
                    File.WriteAllBytes(hashFile, sourceHash);

                    W($"NEW: {sourcePath} ", ConsoleColor.Green);
                    W($"({ToHex(sourceHash)})");
                    WL();
                }
            }

            if(!statement)
                W(" no changes detected.");
            else
                WL("done.");
        }

        private static void WriteStartWork(IEnumerable<string> locations)
        {
            WL();
            WL("Found the following source locations:");
            foreach (var path in locations)
                WL(path);
            WL("Shadow copying changed source files...");
        }

        private static void FilesDiffer(FileSystemInfo destinationInfo, FileSystemInfo sourceInfo, string destinationFile, string sourceFile, string hashFile, byte[] sourceHash, string sourcePath)
        {
            if (destinationInfo.LastWriteTimeUtc > sourceInfo.LastWriteTimeUtc)
            {
                WL($"SKIP: {sourcePath}", ConsoleColor.Yellow);
            }
            else
            {
                File.Copy(sourceFile, destinationFile, true);
                File.WriteAllBytes(hashFile, sourceHash);

                W($"UPDATE: {sourcePath} ", ConsoleColor.Cyan);
                W($"({ToHex(sourceHash)})");
                WL();
            }
        }

        private static byte[] GetChecksum(FileInfo destinationInfo)
        {
            byte[] hashBytes;
            using (var md5 = MD5.Create())
            using (var fs = destinationInfo.OpenRead())
                hashBytes = md5.ComputeHash(fs);
            return hashBytes;
        }

        public static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        private static void Masthead()
        {
            WL(@"
 __             _          
/ _\ __ _ _   _(_)_ __ ___ 
\ \ / _` | | | | | '__/ _ \
_\ \ (_| | |_| | | | |  __/
\__/\__, |\__,_|_|_|  \___|
       |_|                 
", ConsoleColor.Green);
        }

        private static void WL(string value)
        {
            Console.WriteLine(value);
        }

        private static void WL()
        {
            Console.WriteLine();
        }

        private static void W(string value)
        {
            Console.Write(value);
        }

        private static void W(string value, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.Write(value);
            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
        }

        private static void WL(string value, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
        {
            var fore = Console.ForegroundColor;
            var back = Console.BackgroundColor;
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.WriteLine(value);
            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
        }
    }
}
