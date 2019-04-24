﻿// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Blowdart.UI;
using Blowdart.UI.Web;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

public static partial class InlineElements
{
	// TODO this is almost certainly not the right thing: need affinity to be wherever the request/user is
	[ThreadStatic] private static Ui ui;
	internal static void SetUi(Ui threadUi) => ui = threadUi;
	
	// 
	// Indirection Helpers:
	public static Ui br() { return ui.Break(); }
	public static Ui literal(string text) { return ui.Literal(text); }
	public static Ui input(InputType type, object attr = null) { return ui.Input(type, attr); }
	public static Ui submit(string label = "Submit", object attr = null) { return ui.Submit(label, attr); }
	public static Ui fieldset(Action action) { return ui.Fieldset(action); }
	public static Ui form(object attr, Action action) { return ui.Form(attr, action); }
	public static Ui p(string innerText) { return ui.P(innerText); }
	public static bool button(string innerText) { return ui.Button(innerText); }
}