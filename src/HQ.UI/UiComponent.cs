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
using System.Reflection;

namespace HQ.UI
{
	public abstract class UiComponent
	{
		public virtual string Name => null;
		public virtual void Render(Ui ui) { Render(ui, null); }

		public virtual void Render(Ui ui, dynamic model)
		{
			var componentType = GetType();
			if (componentType.BaseType != null && componentType.BaseType.IsConstructedGenericType)
			{
				var modelType = componentType.BaseType.GetGenericArguments()[0];
				var getMethod = new Func<MethodInfo>(() =>
					componentType.GetMethod(nameof(Render), new[] {typeof(Ui), modelType}));
				Internal.TypeExtensions.ExecuteMethodFunction(this, $"{componentType.Name}_{nameof(Render)}_{modelType.Name}",
					getMethod, ui, model);
			}
			else
				throw new NotSupportedException(
					"You need to override `Render(Ui ui, dynamic model)` to call it in this way.");
		}
	}

	/// <inheritdoc />
	public abstract class UiComponent<TModel> : UiComponent
	{
		public abstract void Render(Ui ui, TModel model);
	}
}