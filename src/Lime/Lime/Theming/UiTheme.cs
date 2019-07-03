// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lime.Theming
{
	public class UiTheme
	{
		public static UiTheme Default = new UiTheme();

		public UiTheme()
		{
			ColorVariables = new ColorVariables();
			ImageVariables = new ImageVariables();
		}

		public ColorVariables ColorVariables { get; }
		public ImageVariables ImageVariables { get; }
	}
}