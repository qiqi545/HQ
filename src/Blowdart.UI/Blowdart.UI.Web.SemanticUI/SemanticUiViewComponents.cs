// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using TypeKitchen;

namespace Blowdart.UI.Web.SemanticUI
{
	public sealed class SemanticUiViewComponents : 
		IViewComponent<string>,
		IViewComponent<byte>,
		IViewComponent<byte?>,
		IViewComponent<bool>,
		IViewComponent<bool?>,
		IViewComponent<short>,
		IViewComponent<short?>,
		IViewComponent<long>,
		IViewComponent<long?>,
		IViewComponent<float>,
		IViewComponent<float?>,
		IViewComponent<double>,
		IViewComponent<double?>,
		IViewComponent<decimal>,
		IViewComponent<decimal?>,
		IViewComponent<TimeSpan>,
		IViewComponent<TimeSpan?>,
		IViewComponent<DateTimeOffset>,
		IViewComponent<DateTimeOffset?>,
		IViewComponent<Guid>,
		IViewComponent<Guid?>
	{
		#region Templates

		private static void Number<T>(Ui ui, AccessorMember field, T value)
		{
			InputField("number", ui, field, value);
		}

		private static void Text<T>(Ui ui, AccessorMember field, T value)
		{
			InputField("text", ui, field, value);
		}

		private static void DateTime<T>(Ui ui, AccessorMember field, T value)
		{
			InputField("datetimelocal", ui, field, value);
		}

		private static void Time<T>(Ui ui, AccessorMember field, T value)
		{
			InputField("time", ui, field, value);
		}

		private static void Toggle<T>(Ui ui, AccessorMember field, T value)
		{
			InputField("checkbox", ui, field, value);
		}

		/// <summary><see href="https://semantic-ui.com/elements/input.html"/></summary>
		private static void InputField<T>(string type, Ui ui, AccessorMember field, T value)
		{
			var valueString = value is string s ? s : value?.ToString();

			var properties = field.GetDisplayProperties();
			if (!properties.IsVisible)
				return;

			if (properties.IsHidden)
			{
				RenderInput();
			}
			else
			{
				ui.BeginDiv(properties.IsRequired ? "required field" : "field");
				{
					if (!properties.IsHidden && properties.Label != null)
						ui.Label(properties.Label);

					ui.BeginDiv(GetInputCssClass(properties));
					{
						RenderInput();
					}
					ui.EndDiv();
				}
				ui.EndDiv();
			}

			void RenderInput()
			{
				ui.BeginInput(type: type, name: field.Name, value: valueString,
					placeholder: properties.Placeholder,
					@readonly: properties.IsReadOnly,
					hidden: properties.IsHidden);

				if (properties.Annotation != null)
				{
					ui.BeginDiv("ui basic label");
					ui.Literal(properties.Annotation);
					ui.EndDiv();
				}

				ui.EndInput();
			}
		}

		private static string GetInputCssClass(DisplayProperties properties)
		{
			string inputClass;
			switch (properties.Type)
			{
				case nameof(DataType.Url):
					inputClass = "ui labeled input";
					break;
				default:
					inputClass = properties.Annotation != null ? "ui right labeled input" : "ui input";
					break;
			}

			return inputClass;
		}

		#endregion

		#region Numbers

		public void Render(Ui ui, AccessorMember field, string value)
		{
			Text(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, byte value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, byte? value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, short value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, short? value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, int value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, int? value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, long value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, long? value)
		{
			Number(ui, field, value);
		}
		
		public void Render(Ui ui, AccessorMember field, float value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, float? value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, double value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, double? value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, decimal value)
		{
			Number(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, decimal? value)
		{
			Number(ui, field, value);
		}

		#endregion

		#region Dates & Times

		public void Render(Ui ui, AccessorMember field, DateTimeOffset value)
		{
			DateTime(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, DateTimeOffset? value)
		{
			DateTime(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, DateTime value)
		{
			DateTime(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, DateTime? value)
		{
			DateTime(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, TimeSpan value)
		{
			Time(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, TimeSpan? value)
		{
			Time(ui, field, value);
		}

		#endregion

		#region UUID

		public void Render(Ui ui, AccessorMember field, Guid value)
		{
			Text(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, Guid? value)
		{
			Text(ui, field, value);
		}

		#endregion

		#region Boolean

		public void Render(Ui ui, AccessorMember field, bool value)
		{
			Toggle(ui, field, value);
		}

		public void Render(Ui ui, AccessorMember field, bool? value)
		{
			Toggle(ui, field, value);
		}

		#endregion
	}
}
