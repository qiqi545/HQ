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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using HQ.Extensions.Logging;
using HQ.Platform.Schema.Models;
using Newtonsoft.Json;

namespace HQ.Build
{
    public static class Program
    {
        private const string ApiKeyHeader = "X-API-KEY";
        private const string ApiVersionHeader = "X-API-VERSION";
        private const string VersionHashFile = ApiVersionHeader + ".txt";

        public static void Main(params string[] args)
        {
            Masthead();
            Usage();

            Execute(args, () =>
            {
                ParseCommandLine(args);
            });
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: -c <endpoint> <key> <schemaDirectory> <outputDirectory> <namespace>");
        }

        private static void ParseCommandLine(IReadOnlyCollection<string> args)
        {
            {
                var arguments = new Queue<string>(args);

                while (arguments.Count > 0)
                {
                    string arg = arguments.Dequeue();

                    switch (arg.ToLower())
                    {
                        case "--codegen":
                        case "-c":

                            if (EndOfSubArguments(arguments))
                            {
                                Console.Error.WriteLine("No endpoint specified.");
                                return;
                            }

                            var endpoint = arguments.Dequeue();

                            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
                            {
                                Console.Error.WriteLine("Invalid endpoint.");
                                return;
                            }

                            if (EndOfSubArguments(arguments))
                            {
                                Console.Error.WriteLine("No key specified.");
                                return;
                            }

                            var key = arguments.Dequeue();
                            if (!Guid.TryParse(key, out _))
                            {
                                Console.Error.WriteLine("Invalid key.");
                                return;
                            }

                            var applicationId = arguments.Dequeue();
                            if (!Guid.TryParse(key, out _))
                            {
                                Console.Error.WriteLine("Invalid application ID.");
                                return;
                            }

                            if (EndOfSubArguments(arguments))
                            {
                                Console.Error.WriteLine("No schema directory specified.");
                                return;
                            }

                            var source = arguments.Dequeue();
                            if (!Directory.Exists(source))
                            {
                                Console.Error.WriteLine("Invalid schema directory - does not exist.");
                                return;
                            }

                            var schemas = Directory.GetFiles(source, "*.json");
                            if (schemas.Length == 0)
                            {
                                Console.Error.WriteLine("No schemas found.");
                                return;
                            }

                            List<Schema> manifest;
                            try
                            {
                                manifest = new List<Schema>();
                                foreach (var schema in schemas)
                                {
                                    var text = File.ReadAllText(schema);
                                    var definitions = JsonConvert.DeserializeObject<Schema[]>(text);
                                    manifest.AddRange(definitions);
                                }
                            }
                            catch (Exception)
                            {
                                Console.Error.WriteLine("Invalid schema.");
                                return;
                            }

                            if (EndOfSubArguments(arguments))
                            {
                                Console.Error.WriteLine("No generation directory specified.");
                                return;
                            }

                            var target = arguments.Dequeue();
                            Directory.CreateDirectory(target);

                            if (EndOfSubArguments(arguments))
                            {
                                Console.Error.WriteLine("No application namespace specified.");
                                return;
                            }

                            var appNs = arguments.Dequeue();

                            byte[] payload;
                            try
                            {
                                var wc = new WebClient
                                {
                                    BaseAddress = endpoint,
                                    QueryString = new NameValueCollection
                                    {
                                        ["platformNs"] = "HQ",
                                        ["package"] = "true",
                                        ["appNs"] = appNs,
                                        ["flags"] = CodeGenFlags.All.ToString()
                                    },
                                    Headers = new WebHeaderCollection
                                    {
                                        [ApiKeyHeader] = key,
                                        [HttpRequestHeader.ContentType] = "application/vnd.hq.archivist+json"
                                    }
                                };

                                if (File.Exists(VersionHashFile))
                                {
                                    var versionHash = File.ReadAllText(VersionHashFile);
                                    wc.Headers.Add(ApiVersionHeader, versionHash);
                                }

                                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                                var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(manifest));
                                payload = wc.UploadData($"codegen/{applicationId}", "POST", data);

                                var headers = wc.ResponseHeaders;
                                for (int i = 0; i < headers.Count; i++)
                                {
                                    var name = headers.GetKey(i);
                                    if (!name.Equals(ApiVersionHeader, StringComparison.OrdinalIgnoreCase))
                                        continue;
                                    var value = headers.Get(i);
                                    File.WriteAllText(VersionHashFile, value);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (ex is WebException web)
                                {
                                    if (web.Response is HttpWebResponse http)
                                    {
                                        if (http.StatusCode == HttpStatusCode.NotModified)
                                        {
                                            Console.Out.WriteLine("No schema changes found.");
                                            return;
                                        }
                                    }
                                }

                                Console.Error.WriteLine("Invalid response from service.");
                                Console.Error.WriteLine(ex);
                                return;
                            }

                            using (var ms = new MemoryStream(payload))
                            {
                                var archive = new ZipArchive(ms, ZipArchiveMode.Read);
                                foreach (var entry in archive.Entries)
                                {
                                    var relativePath = entry.FullName;
                                    var targetPath = Path.Combine(target, relativePath);

                                    var targetDir = Path.GetDirectoryName(targetPath);
                                    Directory.CreateDirectory(targetDir);

                                    using (var destination = File.OpenWrite(targetPath))
                                    {
                                        using (var fs = entry.Open())
                                        {
                                            fs.CopyTo(destination);
                                            fs.Flush();
                                        }
                                        destination.Flush();
                                    }
                                }
                            }

                            Console.WriteLine("Service build complete.");

                            break;
                        default:
                            Console.Error.WriteLine("Unrecognized command line parameter (position " +
                                                    (args.Count - arguments.Count - 1) + ")");
                            break;
                    }
                }
            }

            bool EndOfSubArguments(Queue<string> arguments)
            {
                return arguments.Count == 0 || arguments.Peek().StartsWith("-");
            }
        }

        internal static void Execute(string[] args, Action action)
        {
            try
            {
                Trace.Listeners.Add(new ActionTraceListener(Console.Write, Console.WriteLine));

                Console.WriteLine(args == null || args.Length == 0
                    ? "HQ started."
                    : $"HQ started with args: {string.Join(" ", args)}");

                action?.Invoke();

                Console.WriteLine("HQ stopped normally.");
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("HQ stopped unexpectedly. Error: {0}", exception);

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else if (Environment.UserInteractive)
                {
                    Console.WriteLine("Press any key to quit.");
                    Console.ReadKey();
                }
            }
        }

        public static void Masthead()
        {
            // Credit: http://patorjk.com/software/taag/
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(@"
 __   __  _______        ___   _______ 
|  | |  ||       |      |   | |       |
|  |_|  ||   _   |      |   | |   _   |
|       ||  | |  |      |   | |  | |  |
|       ||  |_|  | ___  |   | |  |_|  |
|   _   ||      | |   | |   | |       |
|__| |__||____||_||___| |___| |_______|
");
            Console.ForegroundColor = color;
        }
    }
}
