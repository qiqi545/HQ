// Copyright (c) Daniel Crenna & Contributor. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using HQ.UI;
using HQ.UI.Web;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace HQ.UI.Web
{
	public static partial class InlineElements
	{
		[ThreadStatic] private static Ui ui;
		internal static void SetUi(Ui threadUi) => ui = threadUi;
		internal static Ui GetUi() => ui;

		public static Ui br() { return ui.Break(); }
		public static Ui literal(string text) { return ui.Literal(text); }
		public static Ui _(string text) { return ui._(text); }

		// 
		// Indirection Helpers:
		public static Ui input(InputType type, object attr = null) { return ui.Input(type, attr); }
		public static Ui submit(string label = "Submit", object attr = null) { return ui.Submit(label, attr); }
		public static Ui fieldset(Action action) { return ui.Fieldset(action); }
		public static Ui form(Action<HtmlExtensions.FormAttributes> action) { return ui.Form(action); }
		public static Ui p(string innerText) { return ui.P(innerText); }
		public static Ui button(string innerText, Action<ButtonEvents, ButtonAttributes> events) { return ui.Button(innerText, events); }
		public static int range(int min, int max, int defaultValue, object attr = null) { return ui.Range(min, max, defaultValue, attr); }
	}
}