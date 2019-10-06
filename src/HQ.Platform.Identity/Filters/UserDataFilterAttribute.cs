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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HQ.Data.Contracts;
using HQ.Platform.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using TypeKitchen;

namespace HQ.Platform.Identity.Filters
{
	public class UserDataFilterAttribute : ActionFilterAttribute
	{
		private readonly IUserIdProvider<string> _id;
		private readonly ObjectGetService _service;
		private readonly Type _type;
		private FilterOptions _filter;

		public UserDataFilterAttribute(Type type, ObjectGetService service, IUserIdProvider<string> id)
		{
			_type = type;
			_service = service;
			_id = id;
		}

		private static FilterOptions CreateFilter(Type type, IUserIdProvider<string> id)
		{
			var members = AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			if (!members.ContainsKey("UserId"))
				throw new InvalidOperationException("UserDataFilter Type must have a public property 'UserId'");

			return new FilterOptions
			{
				Fields = new List<Filter>
				{
					new Filter {Field = "UserId", Operator = FilterOperator.Equal, Value = id.Id}
				}
			};
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var user = context.HttpContext.User;
			if (!user.Identity.IsAuthenticated)
				return;

			_filter = _filter ?? CreateFilter(_type, _id);

			// FIXME: handle IEnumerable vs. single gracefully
			var page = await _service.GetAsync(_type, query: null, filter: _filter);
			var data = page.FirstOrDefault();
			if (data == null)
				return;

			var keys = context.ActionArguments.Where(x => x.Value?.GetType() == _type).Select(x => x.Key).ToList();
			foreach (var key in keys)
				context.ActionArguments[key] = data;
		}
	}
}