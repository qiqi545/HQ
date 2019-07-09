﻿using System;
using HQ.UI;
using Microsoft.Extensions.DependencyInjection;
using UIKit;

namespace Lime.UI.iOS
{
	public class UiClient
	{
		private static readonly IServiceCollection Services = new ServiceCollection();
		private static Action<LayoutRoot> _layout;

		public static void Start(string[] args, Action<LayoutRoot> layout)
		{
			_layout = layout;

			UIApplication.Main(args, null, nameof(AppDelegate));
		}

		public static bool FinishedLaunching(UIApplicationDelegate appDelegate)
		{
			UiConfig.Initialize<UiKitSystem>(Services);
			UiConfig.ConfigureServices?.Invoke(Services);

			var serviceProvider = Services.BuildServiceProvider();
			var controller = new UiViewController(serviceProvider);

			UiConfig.Settings = settings => settings.System = new UiKitSystem(controller);

			_layout?.Invoke(serviceProvider.GetRequiredService<LayoutRoot>());
			appDelegate.Window = new UIWindow(UIScreen.MainScreen.Bounds) {RootViewController = controller};
			appDelegate.Window.MakeKeyAndVisible();

			return true;
		}
	}
}