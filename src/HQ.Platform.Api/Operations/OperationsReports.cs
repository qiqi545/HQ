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

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Api.Operations
{
	internal static class OperationsReports
	{
		public class HostedServicesReport
		{
			public List<string> Services { get; set; } = new List<string>();
		}


		public class ServicesReport
		{
			public HashSet<string> MissingRegistrations { get; set; }
			public List<ServiceReport> Services { get; set; }
		}

		public class ServiceReport
		{
			public ServiceLifetime Lifetime { get; set; }
			public string ImplementationType { get; set; }
			public string ImplementationInstance { get; set; }
			public string ImplementationFactory { get; set; }
			public string ServiceType { get; set; }
		}

		public class OptionsReport
		{
			public bool HasErrors { get; set; }
			public List<OptionReport> Options { get; set; }
		}

		public class OptionReport
		{
			public string Scope { get; set; }
			public bool HasErrors { get; set; }
			public List<OptionBindingReport> Values { get; set; }
		}

		public class OptionBindingReport
		{
			public string Type { get; set; }
			public bool IsValid { get; set; }
			public object Value { get; set; }
		}
	}
}