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

using System;
using HQ.Common.MergePatch.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace HQ.Common.MergePatch
{
	public static class MvcBuilderExtensions
	{
		private static void AddJsonMergePatch(this IServiceCollection services,
			Action<JsonMergePatchOptions> configure = null)
		{
			services.AddOptions();
			services.Configure(configure ?? (a => { }));
			services.TryAddEnumerable(ServiceDescriptor
				.Transient<IConfigureOptions<MvcOptions>, JsonMergePatchOptionsSetup>());
		}
	}
}