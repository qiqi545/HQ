// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq.Expressions;
using Blowdart.UI.Scripting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddCors(o => o.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
            }));
            services.AddResponseCompression(o => { o.EnableForHttps = false; });
            services.AddHttpsRedirection(o => { });
            services.AddBlowdartUi(_env);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseCors();
            app.UseResponseCompression();
            app.UseHttpsRedirection();

            app.UseBlowdartUi(_layout);
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
            var builder = WebHost.CreateDefaultBuilder(args).UseStartup<UiServer>();
            var webHost = builder.Build();
            webHost.Run(); // <-- blocks thread
        }
    }
}