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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using HQ.UI.Internal;
using HQ.UI.Internal.Execution;
using Microsoft.Collections.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TypeKitchen;

namespace HQ.UI
{
	public class Ui
	{
		protected Ui(UiData data) => Data = data;

		internal UiSystem System { get; private set; }
		internal UiData Data { get; set; }


		public bool Invalidated { get; private set; }

		public static Ui CreateNew(UiData data)
		{
			return new Ui(data);
		}

		#region System Forwards

		public void Error(string errorMessage, Exception exception = null)
		{
			System.Error(errorMessage, exception);
		}

		#endregion

		public void Invalidate()
		{
			Invalidated = true;
		}

		#region Components

		public void Component(string name)
		{
			var components = Context.UiServices.GetRequiredService<Dictionary<string, Func<UiComponent>>>();
			if (components.TryGetValue(name, out var component))
				component().Render(this);
			else
				Error($"MISSING COMPONENT '{name}'");
		}

		public void Component(string name, dynamic model)
		{
			var components = Context.UiServices.GetRequiredService<Dictionary<string, Func<UiComponent>>>();
			if (components.TryGetValue(name, out var component))
				component().Render(this, model);
			else
				Error($"MISSING COMPONENT '{name}'");
		}

		public void Component<TComponent>() where TComponent : UiComponent
		{
			var components = Context.UiServices.GetRequiredService<Dictionary<Type, Func<Type, UiComponent>>>();
			var componentType = typeof(TComponent);

			if (componentType.IsGenericType)
			{
				var openComponentType = componentType.GetGenericTypeDefinition();
				if (components.TryGetValue(openComponentType, out var component))
					component(componentType).Render(this);
				else
					Error($"MISSING COMPONENT TYPE '{componentType.Name}'");
			}
			else
			{
				if (components.TryGetValue(componentType, out var component))
					component(null).Render(this);
				else
					Error($"MISSING COMPONENT TYPE '{componentType.Name}'");
			}
		}

		public void Component<TComponent>(dynamic model) where TComponent : UiComponent
		{
			var components = Context.UiServices.GetRequiredService<Dictionary<Type, Func<Type, UiComponent>>>();
			var componentType = typeof(TComponent);

			if (componentType.IsGenericType)
			{
				var openComponentType = componentType.GetGenericTypeDefinition();
				if (components.TryGetValue(openComponentType, out var component))
					component(componentType).Render(this, model);
				else
					Error($"MISSING COMPONENT TYPE '{componentType.Name}'");
			}
			else
			{
				if (components.TryGetValue(componentType, out var component))
					component(null).Render(this, model);
				else
					Error($"MISSING COMPONENT TYPE '{componentType.Name}'");
			}
		}

		public void Component<TComponent, TModel>(TModel model) where TComponent : UiComponent
		{
			var components = Context.UiServices.GetRequiredService<Dictionary<Type, Func<Type, UiComponent>>>();
			var componentType = typeof(TComponent);

			if (componentType.IsGenericType)
			{
				var openComponentType = componentType.GetGenericTypeDefinition();
				if (components.TryGetValue(openComponentType, out var component))
					component(componentType).Render(this, model);
				else
					Error($"MISSING COMPONENT TYPE '{componentType.Name}'");
			}
			else
			{
				if (components.TryGetValue(componentType, out var component))
					component(null).Render(this, model);
				else
					Error($"MISSING COMPONENT TYPE '{componentType.Name}'");
			}
		}

		public void Component<TComponent, TController, TModel>()
			where TComponent : UiComponent<TModel>
			where TModel : class
		{
			var components = Context.UiServices.GetRequiredService<Dictionary<Type, Func<Type, UiComponent>>>();
			if (components.TryGetValue(typeof(TComponent), out var component))
			{
				var settings = Context.UiServices.GetRequiredService<UiSettings>();

				var model = Data.GetModel<TController, TModel>(settings.DefaultPageMethodName, this);

				if (component is UiComponent<TModel> typed)
					typed.Render(this, model);
				else
					component(null).Render(this, model);
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

		internal void View(Type type, object model)
		{
			var accessor = ReadAccessor.Create(model);

			var components = Context.UiServices.GetRequiredService<Dictionary<Type, Func<IViewComponent>>>();

			while (true)
			{
				if (Attribute.IsDefined(type, typeof(ViewComponentAttribute)))
				{
					var attribute =
						(ViewComponentAttribute) Attribute.GetCustomAttribute(type, typeof(ViewComponentAttribute));
					if (components.ContainsKey(attribute.Type))
					{
						type = attribute.Type;
						continue;
					}
				}

				var members = AccessorMembers.Create(type, AccessorMemberScope.Public, AccessorMemberTypes.Properties);

				foreach (var member in members)
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
								throw new NullReferenceException(
									$"HQ.UI did not register a default view for '{member.Type.Name}'");
							}

							//
							// Nested View:
							if (accessor.TryGetValue(model, member.Name, out var value)) View(member.Type, value);
						}
						else
						{
							//
							// Default View:
							if (accessor.TryGetValue(model, member.Name, out var value))
								Render(defaultView, member, value);
						}

						continue;
					}

					//
					// Custom View:
					if (components.TryGetValue(custom.Type, out var customView))
					{
						var memberAccessor = ReadAccessor.Create(member.Type);
						if (memberAccessor.TryGetValue(model, member.Name, out var value))
							Render(customView, member, value);
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
				executor.Execute(view, new[] {this, member, value});

				// var caller = CallAccessor.Create(method);
				// caller.Call(view, this, member, value);
			}
		}

		#endregion

		#region Lifecycle

		private int _count;

		internal readonly MultiValueDictionary<string, Value128> Events =
			MultiValueDictionary<string, Value128>.Create<HashSet<Value128>>();

		internal readonly Dictionary<Value128, int> InputValues = new Dictionary<Value128, int>();

		public UiContext Context { get; private set; }

		public void Begin(UiSystem system, UiContext context)
		{
			_count = 0;
			Context = context;
			Events.Clear();
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

		public Value128 NextId(string id = null, [CallerMemberName] string callerMemberName = null)
		{
			NextIdHash = Hashing.MurmurHash3(id ?? $"{callerMemberName}{_count++}", NextIdHash) ^ NextIdHash;
			return NextIdHash;
		}

		public Value128 NextId(StringBuilder id)
		{
			NextIdHash = Hashing.MurmurHash3(id, NextIdHash) ^ NextIdHash;
			return NextIdHash;
		}

		public Value128 NextId(int i)
		{
			NextIdHash = Hashing.MurmurHash3((ulong) i, NextIdHash) ^ NextIdHash;
			return NextIdHash;
		}

		#endregion
	}
}