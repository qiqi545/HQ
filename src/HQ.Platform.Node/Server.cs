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
using System.Diagnostics;
using HQ.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace HQ.Platform.Node
{
    public static class Server
    {
        public static void Start<TStartup>(string[] args)
            where TStartup : class
        {
            Masthead();

            Execute(args, () =>
            {
                var builder = WebHost.CreateDefaultBuilder(args);
                builder.ConfigureHq(args, false);
                builder.UseStartup<TStartup>();

                var host = builder.Build();
                host.Run();
            });
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
    }
}
