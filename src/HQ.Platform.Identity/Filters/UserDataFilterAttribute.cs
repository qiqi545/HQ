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
		private readonly Type _type;
		private readonly ObjectGetService _service;
		private readonly IUserIdProvider _id;
		private FilterOptions _filter;

		public UserDataFilterAttribute(Type type, ObjectGetService service, IUserIdProvider id)
		{
			_type = type;
			_service = service;
			_id = id;
		}

		private static FilterOptions CreateFilter(Type type, IUserIdProvider id)
		{
			var members = AccessorMembers.Create(type, AccessorMemberTypes.Properties, AccessorMemberScope.Public);
			if (!members.ContainsKey("UserId"))
				throw new InvalidOperationException("UserDataFilter Type must have a public property 'UserId'");

			return new FilterOptions
			{
				Fields = new List<Filter>
				{
					new Filter {
						Field = "UserId",
						Operator = FilterOperator.Equal,
						Value = id.Id
					}
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
