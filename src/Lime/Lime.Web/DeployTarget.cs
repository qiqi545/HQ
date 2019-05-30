// Copyright (c) Daniel Crenna & Contributor. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Lime.Web
{
	public enum DeployTarget : byte
	{
		/// <summary>
		/// The server renders the UI and manages application state.
		/// </summary>
		Server,

		/// <summary>
		/// The client renders the UI and the server manages application state.
		///
		/// Calls that should run on the server are invoked via actions:
		/// <code>
		///		ui.RunAtServer(()=> ...);
		/// </code>
		/// </summary>
		Client,

		/// <summary>
		/// The client renders the UI, and there is no application state.
		/// </summary>
		Static
	}
}