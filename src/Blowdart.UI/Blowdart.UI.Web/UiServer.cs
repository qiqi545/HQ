// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
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
            AddUiResources(services);
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

		#region Start Methods

		public static void Start<TStartup>(string[] args, Action<LayoutRoot> layout)
        {
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

			_startup = typeof(TStartup);
            Start(args, layout);
        }

        public static void Start<TStartup>(string[] args, string filePath = "ui.csx")
        {
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

			_startup = typeof(TStartup);
            Start(args, filePath);
        }

        public static void Start<TStartup, TService, TComponent>(string[] args) where TComponent : UiComponent
        {
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

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
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

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
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

			_layout = layout;
			StartServer(args);
        }

        public static void Start(string[] args, string filePath /*= "ui.csx"*/)
        {
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

			if (!string.IsNullOrWhiteSpace(filePath))
            {
                _layout = layout =>
                {
                    var options = ScriptOptions.Default
                        .WithReferences(typeof(Console).Assembly, typeof(MemberExpression).Assembly)
                        .WithImports("System", "System.Linq.Expressions");

                    var script = CSharpScript.Create(File.OpenRead(filePath), options, typeof(ScriptContext));
                    var context = new ScriptContext { layout = layout };
                    script.RunAsync(context).ConfigureAwait(false).GetAwaiter().GetResult();
                };
            }

            Start(args);
        }

        public static void Start(string[] args)
        {
			_callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
			if (_callerType == null)
				throw new ArgumentException("You cannot start a UiServer from an anonymous method.");

			StartServer(args);
        }

		#endregion

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

            AddImplicitHandlers(webHost.Services);

			var cancellationToken = new CancellationToken();
            
            // webHost.Run(); 
            using (webHost)
            {
                webHost.StartAsync(cancellationToken).GetAwaiter().GetResult();
                var serviceProvider = webHost.Services;

                var layout = serviceProvider.GetRequiredService<LayoutRoot>();
                var meta = Caches.Introspection.IntrospectMeta();
                var systems = Caches.Introspection.IntrospectSystems();

				foreach (var handler in Handlers)
                {
	                layout.AddHandler(handler.Key, handler.Value);
	                layout.AddMeta(handler.Key, meta[handler.Value]);
	                layout.AddSystem(handler.Key, systems[handler.Value]);
                }

				HandlersAreFinished();
				
                var service = serviceProvider.GetService<IHostingEnvironment>();
                if (!serviceProvider.GetRequiredService<WebHostOptions>().SuppressStatusMessages)
                {
                    Console.WriteLine($"Hosting environment: {service.EnvironmentName}");
                    Console.WriteLine($"Content root path: {service.ContentRootPath}");
                    var addresses = webHost.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
                    if (addresses != null)
						foreach (var address in addresses)
							Console.WriteLine($"Now listening on: {address}");

					Console.WriteLine("Application started. Press Ctrl+C to shut down.");
                }
                
                OnStarted(webHost, config);

                var appLifetime = serviceProvider.GetService<IApplicationLifetime>();
                cancellationToken.Register(state => ((IApplicationLifetime)state).StopApplication(), appLifetime);
                var completionSource = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                appLifetime.ApplicationStopping.Register(obj => ((TaskCompletionSource<object>)obj).TrySetResult(null), completionSource);
                completionSource.Task.GetAwaiter().GetResult();
                webHost.StopAsync(cancellationToken).GetAwaiter().GetResult();
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
                startInfo.Arguments = "/C start " + url;
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

        #region IMGUI

        private static bool _finishedAddingHandlers;

        private static readonly Dictionary<string, MethodInfo> Handlers = new Dictionary<string, MethodInfo>();
        
        private static void AddImplicitHandlers(IServiceProvider serviceProvider)
        {
            HandlersAreNotFinished();

			//
			// Annotated:
            foreach (var handler in Caches.Introspection.IntrospectHandlers())
                AddHandler(handler.Key, handler.Value);

			//
			// Conventional Default (entry class has a default method):
			_callerMethods = _callerMethods ?? IntrospectHostMethods();
			var settings = serviceProvider.GetRequiredService<UiSettings>();
			if (_callerMethods.TryGetValue(settings.DefaultPageMethodName, out var method))
				AddHandler("/", method);
		}

        private static void HandlersAreFinished()
        {
            if(_finishedAddingHandlers)
                throw new InvalidOperationException($"{nameof(HandlersAreFinished)} was already called");

            Handlers.Clear();

            Caches.Introspection.Clear();
            
            _callerMethods?.Clear();

            _finishedAddingHandlers = true;
        }

        private static void HandlersAreNotFinished()
        {
            if(_finishedAddingHandlers)
                throw new InvalidOperationException("Handlers are finalized");
        }

        public static void AddHandler(string template, MethodInfo method)
        {
            HandlersAreNotFinished();

            if (Handlers.ContainsKey(template))
            {
                Trace.TraceWarning($"Replacing existing handler for template '{template}'. Defer adding handlers until they represent a canonical set, to avoid this warning.");
                Handlers.Remove(template);
            }

            Handlers.Add(template, method);
        }

        public static void AddHandler(string template, string handlerNameOrMethodName)
        {
            HandlersAreNotFinished();

            // first look for non-template qualified handler disambiguation
            var handlerMethodsWithName = Caches.Introspection.IntrospectHandlerNames();
            if (handlerMethodsWithName.TryGetValue(handlerNameOrMethodName, out var method))
            {
                AddHandler(template, method);
                return;
            }

			// resort to matching the method by stack frame
            _callerMethods = _callerMethods ?? IntrospectHostMethods();
            if (!_callerMethods.TryGetValue(handlerNameOrMethodName, out method))
	            return;

            // there are less error-prone ways to do this, educate!
            Trace.TraceWarning($"Handler '{handlerNameOrMethodName}' was added by stack frame. You can explicitly define the handler name via HandlerNameAttribute, or the complete Handler definition via HandlerAttribute, to avoid this message.");

            AddHandler(template, method);
        }

        private static Type _callerType;
        private static Dictionary<string, MethodInfo> _callerMethods;
        private static Dictionary<string, MethodInfo> IntrospectHostMethods()
        {
	        _callerType = _callerType ?? new StackFrame(1).GetMethod().DeclaringType;
	        if (_callerType == null)
		        throw new ArgumentException("You cannot add a handler in this way from an anonymous method.");

			return _callerType.GetMethods()
                .Select(x => new KeyValuePair<string, MethodInfo>(x.Name, x))
                .ToDictionary(k => k.Key, v => v.Value);
        }

        private static void AddUiResources(IServiceCollection services)
        {
			var methods = Caches.Introspection.IntrospectMethods();
			var kvp = methods.Where(x => Attribute.IsDefined(x, typeof(UiSystemAttribute))).Select(x => (UiSystemAttribute) Attribute.GetCustomAttribute(x, typeof(UiSystemAttribute))).Distinct();
            foreach (var entry in kvp)
                services.AddUiResources(entry.Type.Assembly);
        }

        #endregion
    }
}