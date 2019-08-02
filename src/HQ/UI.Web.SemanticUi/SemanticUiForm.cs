﻿#region LICENSE

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
using System.Collections;
using System.ComponentModel.DataAnnotations;
using TypeKitchen;

namespace HQ.UI.Web.SemanticUi
{
	public sealed class SemanticUiForm :
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
		IViewComponent<Guid?>,
		IViewComponent<ICollection>
	{
		public void Render(Ui ui, AccessorMember field, ICollection value)
		{
			foreach (var item in value)
			{
				var type = item.GetType();

				ui.View(type, item);
			}
		}

		public void BeforeView(Ui ui)
		{
			ui.BeginForm("ui form");
		}

		public void AfterView(Ui ui)
		{
			ui.BeginButton("ui button", type: "submit")
				.Literal("Submit")
				.EndButton();

			ui.EndForm();
		}

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

		/// <summary>
		///     <see href="https://semantic-ui.com/elements/input.html" />
		/// </summary>
		private static void InputField<T>(string type, Ui ui, AccessorMember field, T value)
		{
			var valueString = value is string s ? s : value?.ToString();

			var properties = field.GetDisplayProperties();
			if (!properties.IsVisible)
				return;

			if (properties.IsHidden)
				RenderInput();
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