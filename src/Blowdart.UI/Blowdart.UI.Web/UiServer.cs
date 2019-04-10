// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Blowdart.UI.Internal;
using Blowdart.UI.Scripting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Blowdart.UI.Web
{
    public class UiServer
    {
        private static Action<LayoutRoot> _layout;
        private static Type _startup;
        private readonly IHostingEnvironment _env;

        public UiServer(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (Standalone)
            {
                services.AddRouting(options =>
                {
                    options.LowercaseUrls = true;
                    options.LowercaseQueryStrings = true;
                    options.AppendTrailingSlash = true;
                });
                services.AddCors(o => o.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                }));
                services.AddResponseCompression(o => { o.EnableForHttps = false; });
                services.AddHttpsRedirection(o => { });
            }
            services.AddBlowdartUi(_env);
            _startup?.ExecuteMethod(nameof(ConfigureServices), services);
        }

        private static bool Standalone => _startup == null;

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (Standalone)
            {
                app.UseCors();
                app.UseResponseCompression();
                if (env.IsDevelopment())
                    app.UseDeveloperExceptionPage();
                else
                    app.UseHsts();
                app.UseHttpsRedirection();
            }
           
            app.UseBlowdartUi(_layout);
            _startup?.ExecuteMethod(nameof(Configure), app, env);
        }

        public static void Start<TStartup>(string[] args, Action<LayoutRoot> layout)
        {
            _startup = typeof(TStartup);
            Start(args, layout);
        }

        public static void Start<TStartup>(string[] args, string filePath = "ui.csx")
        {
            _startup = typeof(TStartup);
            Start(args, filePath);
        }

        public static void Start<TStartup, TService, TComponent>(string[] args) where TComponent : UiComponent
        {
            Start<TStartup>(args, site =>
            {
                site.Default<TService>((ui, model) =>
                {
                    ui.Component<TComponent>(model);
                });
            });
        }

        public static void Start<TStartup, TService, TComponent, TModel>(string[] args) where TComponent : UiComponent<TModel>
        {
            Start<TStartup>(args, site =>
            {
                site.Default<TService>((ui, model) =>
                {
                    ui.Component<TComponent>(model);
                });
            });
        }

        public static void Start(string[] args, Action<LayoutRoot> layout)
        {
            _layout = layout;
            StartServer(args);
        }

        public static void Start(string[] args, string filePath = "ui.csx")
        {
            _layout = layout =>
            {
                var options = ScriptOptions.Default
                    .WithReferences(typeof(Console).Assembly, typeof(MemberExpression).Assembly)
                    .WithImports("System", "System.Linq.Expressions");

                var script = CSharpScript.Create(File.OpenRead(filePath), options, typeof(ScriptContext));
                var context = new ScriptContext {layout = layout};
                script.RunAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
            };

            StartServer(args);
        }

        private static void StartServer(string[] args)
        {
            Masthead();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("blowdart.json", optional: true)
                .Build();
            
            if (!string.IsNullOrEmpty(config["applicationUrls"]))
                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", config["applicationUrls"]);

            var environmentVariables = config.GetSection("environmentVariables");
            foreach (var entry in environmentVariables.AsEnumerable())
            {
                if (entry.Value == null)
                    continue;
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }

            var builder = WebHost.CreateDefaultBuilder(args);

            builder.UseConfiguration(new ConfigurationBuilder()
                .AddConfiguration(config)
                .AddCommandLine(args).Build());

            builder.UseStartup<UiServer>();
            var webHost = builder.Build();

            var token = new CancellationToken();
            
            // webHost.Run(); 
            using (webHost)
            {
                webHost.StartAsync(token).GetAwaiter().GetResult();
                var service = webHost.Services.GetService<IHostingEnvironment>();
                if (!webHost.Services.GetRequiredService<WebHostOptions>().SuppressStatusMessages)
                {
                    Console.WriteLine("Hosting environment: " + service.EnvironmentName);
                    Console.WriteLine("Content root path: " + service.ContentRootPath);
                    ICollection<string> addresses = webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
                    if (addresses != null)
                    {
                        foreach (var address in addresses)
                            Console.WriteLine("Now listening on: " + address);
                    }
                    Console.WriteLine("Application started. Press Ctrl+C to shut down.");
                }
                
                OnStarted(webHost, config);

                var appLifetime = webHost.Services.GetService<IApplicationLifetime>();
                token.Register(state => ((IApplicationLifetime)state).StopApplication(), appLifetime);
                var completionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                appLifetime.ApplicationStopping.Register(obj => ((TaskCompletionSource<object>)obj).TrySetResult(null), completionSource);
                completionSource.Task.GetAwaiter().GetResult();
                webHost.StopAsync(token).GetAwaiter().GetResult();
            }
        }

        private static void OnStarted(IWebHost webHost, IConfiguration config)
        {
            var feature = webHost.ServerFeatures.Get<IServerAddressesFeature>();
            
            bool.TryParse(config["launchBrowser"], out var launchBrowser);

            if (!launchBrowser)
                return;

            var url = feature.Addresses.FirstOrDefault(x => x.StartsWith("https://")) ?? feature.Addresses.First();

            if (IPAddress.TryParse(url, out var address) && IPAddress.IsLoopback(address))
                url = url.Replace(address.ToString(), "localhost");

            var startInfo = new ProcessStartInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo.FileName = "cmd";
                startInfo.Arguments = ("/C start " + url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo.FileName = "open";
                startInfo.Arguments = url;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo.FileName = "xdg-open";
                startInfo.Arguments = url;
            }
            else
            {
                return;
            }

            Process.Start(startInfo);
        }

        private static void Masthead()
        {
            // Credit: http://patorjk.com/software/taag/
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"Powered By:
 __        __        __        __  ___         
|__) |    /  \ |  | |  \  /\  |__)  |   |  | | 
|__) |___ \__/ |/\| |__/ /~~\ |  \  |  .\__/ |                                               
");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}