#region LICENSE

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

using Microsoft.AspNetCore.Http;

namespace HQ.UI.Web
{
	public class UiServerOptions
	{
		public PathString HubPath { get; set; } = "/ui";
		public PathString LoggingPath { get; set; } = "/server/logs";
		public PathString TemplatePath { get; set; } = "/lib/index.html";
		public string ContentType { get; set; } = "text/html";
		public string BodyElementId { get; set; } = "ui-body";
		public string ScriptElementId { get; set; } = "ui-scripts";
		public bool UsePrerendering { get; set; } = true;
		public bool UseLogStreaming { get; set; } = true;
		public ServerTransport MessagingModel { get; set; } = ServerTransport.All;
		public DeployTarget DeployTarget { get; set; } = DeployTarget.Server;
	}
}