// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Blowdart.UI
{
    public class Ui : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        protected Ui(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            System = serviceProvider.GetRequiredService<UiSystem>();
        }

        internal UiSystem System { get; }

        public static Ui CreateNew(IServiceProvider serviceProvider)
        {
            return new Ui(serviceProvider);
        }

        #region Components

        public void Component(string name, dynamic model = null)
        {
            var components = _serviceProvider.GetRequiredService<Dictionary<string, UiComponent>>();
            if (components.TryGetValue(name, out var component))
                component.Render(this, model);
            else
                Error($"MISSING COMPONENT '{name}'");
        }

        public void Component<TComponent>(dynamic model = null) where TComponent : UiComponent
        {
            var components = _serviceProvider.GetRequiredService<Dictionary<Type, UiComponent>>();
            if (components.TryGetValue(typeof(TComponent), out var component))
                component.Render(this, model);
            else
                Error($"MISSING COMPONENT TYPE '{typeof(TComponent).Name}'");
        }

        #endregion

        #region Lifecycle

        private int _count;
        public readonly HashSet<Value128> Clicked = new HashSet<Value128>();

        public void Begin()
        {
            _count = 0;
            Clicked.Clear();
            System.Begin();
        }

        public void End()
        {
            System.End();
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

        public bool Button(string text)
        {
            return System.Button(this, text);
        }

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