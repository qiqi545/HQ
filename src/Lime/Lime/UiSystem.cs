// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;

namespace Lime
{
	public abstract class UiSystem
	{
		public abstract void Begin(UiContext context = null);
		public abstract void End();

		public virtual void Error(string errorMessage, Exception exception = null)
		{
			Trace.WriteLine($"UI error: {errorMessage} {(exception == null ? "" : $"{exception}")}");
		}

		public virtual void PopulateAction(Ui ui, UiSettings settings, UiAction action, string template, object target,
			MethodInfo callee = null)
		{
			action.MethodName = callee?.Name ?? template ?? settings.DefaultPageMethodName;
			action.Arguments = null;
		}
	}
}