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
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace HQ.Data.Contracts.Mvc
{
	public class MustHaveQueryParametersAttribute : Attribute, IActionConstraint
	{
		private readonly string[] _matchAll;

		public MustHaveQueryParametersAttribute(params string[] matchAll) => _matchAll = matchAll;

		public int Order => 0;

		public bool Accept(ActionConstraintContext context)
		{
			var query = context.RouteContext.HttpContext.Request.Query;
			if (query.Count != _matchAll.Length) return false;

			foreach (var key in _matchAll)
				if (!query.ContainsKey(key))
					return false;

			return true;
		}
	}
}