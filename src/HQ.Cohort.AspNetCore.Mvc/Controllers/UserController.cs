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

using System.Linq;
using System.Threading.Tasks;
using HQ.Cohort.Models;
using HQ.Common.AspNetCore;
using HQ.Tokens;
using HQ.Tokens.AspNetCore.Mvc.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HQ.Cohort.AspNetCore.Mvc.Controllers
{
    [Route("users")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [RequireClaim(ClaimTypes.Permission, ClaimValues.ManageUsers)]
    public class UserController<TUser> : Controller where TUser : IdentityUser
    {
        private readonly IUserService<TUser> _userService;

        public UserController(IUserService<TUser> userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userService.Users;
            // ReSharper disable once UseMethodAny.2 
            if (users.Count() == 0)
                return NotFound();
            return Ok(users);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateUserModel model)
        {
            if (!this.TryValidateModelState(out var error))
                return error;

            var result = await _userService.CreateAsync(model);

            return result.Succeeded
                ? Created("/api/users/" + result.Data.Id, result.Data)
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetRoles([FromRoute] string id)
        {
            var user = await FindById(id);
            if (user == null)
                return NotFound();

            var result = await _userService.GetRolesAsync(user);
            if (result.Data == null || result.Data.Count == 0)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("{id}/roles/{role}")]
        public async Task<IActionResult> AddToRole([FromRoute] string id, [FromRoute] string role)
        {
            var user = await FindById(id);
            if (user == null)
                return NotFound();

            var result = await _userService.AddToRoleAsync(user, role);

            return result.Succeeded
                ? Created($"/api/users/{user.Id}/roles", user)
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpDelete("{id}/roles/{role}")]
        public async Task<IActionResult> RemoveFromRole([FromRoute] string id, [FromRoute] string role)
        {
            var user = await FindById(id);
            if (user == null)
                return NotFound();

            var result = await _userService.RemoveFromRoleAsync(user, role);

            return result.Succeeded
                ? Ok()
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        public async Task<TUser> FindById([FromRoute] string id)
        {
            var user = await _userService.FindByIdAsync(id);
            return user.Data;
        }

        [HttpGet("{id}/email")]
        public async Task<TUser> FindByEmail([FromRoute] string email)
        {
            var user = await _userService.FindByEmailAsync(email);
            return user.Data;
        }

        [HttpGet("{id}/username")]
        public async Task<TUser> FindByUsername([FromRoute] string username)
        {
            var user = await _userService.FindByNameAsync(username);
            return user.Data;
        }
    }
}
