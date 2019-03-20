// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq.Expressions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Blowdart.UI.Internal;
using Blowdart.UI.Scripting;

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
            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<UiServer>();
            var webHost = builder.Build();
            webHost.Run(); // <-- blocks thread
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