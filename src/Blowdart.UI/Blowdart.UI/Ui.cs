// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Blowdart.UI.Internal;
using Microsoft.Extensions.DependencyInjection;

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