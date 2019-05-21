// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using TypeKitchen;

namespace Blowdart.UI.Web
{
	/// <summary>
	/// Built-in view components for the web.
	/// </summary>
	public sealed class ViewComponents : 
		IViewComponent<string>,
		IViewComponent<byte>,
		IViewComponent<byte?>,
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
		public void Render(Ui ui, AccessorMember field, string value)
		{
			ui.BeginInput(type: "text");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, byte value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, byte? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, short value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, short? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, int value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, int? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, long value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, long? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, float value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, float? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, double value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, double? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, decimal value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, decimal? value)
		{
			ui.BeginInput(type: "number");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, DateTimeOffset value)
		{
			ui.BeginInput(type: "datetimelocal");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, DateTimeOffset? value)
		{
			ui.BeginInput(type: "datetimelocal");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, DateTime value)
		{
			ui.BeginInput(type: "datetimelocal");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, DateTime? value)
		{
			ui.BeginInput(type: "datetimelocal");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, TimeSpan value)
		{
			ui.BeginInput(type: "time");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, TimeSpan? value)
		{
			ui.BeginInput(type: "time");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, Guid value)
		{
			ui.BeginInput(type: "text");
			ui.EndInput();
		}

		public void Render(Ui ui, AccessorMember field, Guid? value)
		{
			ui.BeginInput(type: "text");
			ui.EndInput();
		}
	}
}
