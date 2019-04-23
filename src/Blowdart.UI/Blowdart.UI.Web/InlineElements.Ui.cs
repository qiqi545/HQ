// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Blowdart.UI;
using Blowdart.UI.Web;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

public static partial class InlineElements
{
	public static Ui ui;

	public static bool button(string text, object attr)
	{
		return ui.Button(text, attr);
	}

	public static void br() { ui.Break(); }
}