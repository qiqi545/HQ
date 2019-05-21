// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Blowdart.UI.Internal;
using Blowdart.UI.Internal.Execution;
using Microsoft.Extensions.DependencyInjection;
using TypeKitchen;

namespace Blowdart.UI
{
    public class Ui : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        protected Ui(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Data = serviceProvider.GetRequiredService<UiData>();
        }

        internal UiSystem System { get; private set; }
        internal UiData Data { get; set; }

        public static Ui CreateNew(IServiceProvider serviceProvider)
        {
            return new Ui(serviceProvider);
        }

        #region Components

        public void Component(string name)
        {
			var components = _serviceProvider.GetRequiredService<Dictionary<string, Func<UiComponent>>>();
			if (components.TryGetValue(name, out var component))
                component().Render(this);
            else
                Error($"MISSING COMPONENT '{name}'");
        }

        public void Component(string name, dynamic model)
        {
			var components = _serviceProvider.GetRequiredService<Dictionary<string, Func<UiComponent>>>();
			if (components.TryGetValue(name, out var component))
                component().Render(this, model);
            else
                Error($"MISSING COMPONENT '{name}'");
        }

        public void Component<TComponent>() where TComponent : UiComponent
        {
            var components = _serviceProvider.GetRequiredService<Dictionary<Type, Func<UiComponent>>>();
            if (components.TryGetValue(typeof(TComponent), out var component))
                component().Render(this);
            else
                Error($"MISSING COMPONENT TYPE '{typeof(TComponent).Name}'");
        }

        public void Component<TComponent>(dynamic model) where TComponent : UiComponent
        {
            var components = _serviceProvider.GetRequiredService<Dictionary<Type, Func<UiComponent>>>();
            if (components.TryGetValue(typeof(TComponent), out var component))
                component().Render(this, model);
            else
                Error($"MISSING COMPONENT TYPE '{typeof(TComponent).Name}'");
        }

        public void Component<TComponent, TService, TModel>() 
            where TComponent : UiComponent<TModel>
            where TModel : class
        {
            var components = _serviceProvider.GetRequiredService<Dictionary<Type, Func<UiComponent>>>();
            if (components.TryGetValue(typeof(TComponent), out var component))
            {
	            var settings = _serviceProvider.GetRequiredService<UiSettings>();
				
				var model = Data.GetModel<TService, TModel>(settings.DefaultPageMethodName);

				if (component is UiComponent<TModel> typed)
                    typed.Render(this, model);
                else
                    component().Render(this, model);
            }
            else
                Error($"MISSING COMPONENT TYPE '{typeof(TComponent).Name}'");
        }

		#endregion

		#region Views

		public void View<TModel>(TModel model)
		{
			View(typeof(TModel), model);
		}

		private void View(Type type, object model)
		{
			var accessor = ReadAccessor.Create(model);

			var components = _serviceProvider.GetRequiredService<Dictionary<Type, Func<IViewComponent>>>();

			while (true)
			{
				var fields = AccessorMembers.Create(type, AccessorMemberScope.Public, AccessorMemberTypes.Properties);

				if (Attribute.IsDefined(type, typeof(ViewComponentAttribute)))
				{
					var attribute = (ViewComponentAttribute) Attribute.GetCustomAttribute(type, typeof(ViewComponentAttribute));
					if (components.ContainsKey(attribute.Type))
					{
						type = attribute.Type;
						continue;
					}
				}

				foreach (var member in fields)
				{
					if (!member.TryGetAttribute(out ViewComponentAttribute custom))
					{
						if (!components.TryGetValue(member.Type, out var defaultView))
						{
							if (member.Type.IsValueTypeOrNullableValueType())
							{
#if DEBUG
								if (Debugger.IsAttached) Debugger.Break();
#endif
								throw new NullReferenceException($"Blowdart did not register a default view for '{member.Type.Name}'");
							}

							//
							// Nested View:
							if (accessor.TryGetValue(model, member.Name, out var value))
							{
								View(member.Type, value);
							}
						}
						else
						{
							//
							// Default View:
							if (accessor.TryGetValue(model, member.Name, out var value))
							{
								Render(defaultView, member, value);
							}
						}

						continue;
					}

					//
					// Custom View:
					if (components.TryGetValue(custom.Type, out var customView))
					{
						var memberAccessor = ReadAccessor.Create(member.Type);
						if (memberAccessor.TryGetValue(model, member.Name, out var value))
						{
							Render(customView, member, value);
						}
					}
				}

				break;
			}

			void Render(Func<IViewComponent> viewFunc, AccessorMember member, object value)
			{
				var view = viewFunc();
				var method = view.GetType().GetMethod(nameof(IViewComponent<object>.Render),
					new[] {typeof(Ui), typeof(AccessorMember), member.Type});

				// TODO replace me
				var executor = ObjectMethodExecutor.Create(method, view.GetType().GetTypeInfo());
				executor.Execute(view, new [] { this, member, value });

				// var caller = CallAccessor.Create(method);
				// caller.Call(view, this, member, value);
			}
		}

		#endregion

		#region Lifecycle

		private int _count;
        internal readonly HashSet<Value128> MouseOut = new HashSet<Value128>();
		internal readonly HashSet<Value128> MouseOver = new HashSet<Value128>();
		internal readonly HashSet<Value128> Clicked = new HashSet<Value128>();
        internal readonly Dictionary<Value128, int> InputValues = new Dictionary<Value128, int>();

		public UiContext Context { get; private set; }

        public void Begin(UiSystem system, UiContext context)
        {
            _count = 0;
            Context = context;
            MouseOver.Clear();
            MouseOut.Clear();
			Clicked.Clear();
			InputValues.Clear();
            System = system;
            System.Begin(context);
        }

        public void End()
        {
            System.End();
            Context.Clear();
        }

        #endregion

        #region Hashing 

        internal Value128 NextIdHash;

        public void NextId(string id = null, [CallerMemberName] string callerMemberName = null)
        {
            NextIdHash = Hashing.MurmurHash3(id ?? $"{callerMemberName}{_count++}", NextIdHash) ^ NextIdHash;
        }

        public void NextId(StringBuilder id)
        {
            NextIdHash = Hashing.MurmurHash3(id, NextIdHash) ^ NextIdHash;
        }

        public void NextId(int i)
        {
            NextIdHash = Hashing.MurmurHash3((ulong) i, NextIdHash) ^ NextIdHash;
        }

        #endregion

        #region System Forwards

        public void Error(string errorMessage, Exception exception = null)
        {
            System.Error(errorMessage, exception);
        }

        #endregion

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }
    }
}