// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

namespace Blowdart.UI.Web
{
	public static class Events
	{
		public const string abort = "abort";
		public const string afterprint = "afterprint";
		public const string beforeprint = "beforeprint";
	}

	public static class MouseEvents
	{
		public const string click = "click";
		public const string contextmenu = "contextmenu";
		public const string dblclick = "click";
		public const string mousedown = "mousedown";
		public const string mouseenter = "mouseenter";
		public const string mouseleave = "mouseleave";
		public const string mousemove = "mousemove";
		public const string mouseout = "mouseout";
		public const string mouseover = "mouseover";
		public const string mouseup = "mouseup";
	}

	public static class KeyboardEvents
	{
		public const string keydown = "keydown";
		public const string keypress = "keypress";
		public const string keyup = "keyup";
	}

	public static class FocusEvents
	{
		public const string blur = "blur";
		public const string focus = "focus";
		public const string focusin = "focusin";
		public const string focusout = "focusout";
	}

	public static class InputEvents
	{
		public const string input = "input";

	
		public const string change = "change";
		
		public const string invalid = "invalid";
	}

	public static class ClipboardEvents
	{
		public const string copy = "copy";
		public const string cut = "cut";
		public const string paste = "paste";
	}
}