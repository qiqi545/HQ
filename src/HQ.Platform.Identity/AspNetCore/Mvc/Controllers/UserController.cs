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
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.AspNetCore.Mvc.Controllers
{
    [Route("users")]
    [DynamicController(typeof(IdentityApiOptions))]
    [DynamicAuthorize(typeof(IdentityApiOptions), nameof(IdentityApiOptions.Policies), nameof(IdentityApiOptions.Policies.Users))]
	[ApiExplorerSettings(IgnoreApi = false)]
	[MetaCategory("Identity", "Manages application access controls.")]
    [DisplayName("Users")]
    [MetaDescription("Manages user accounts.")]
    public class UserController<TUser, TTenant, TKey> : DataController
        where TUser : IdentityUserExtended<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IOptions<IdentityApiOptions> _options;
        private readonly ITenantService<TTenant> _tenantService;
        private readonly IUserService<TUser> _userService;

        public UserController(IUserService<TUser> userService, ITenantService<TTenant> tenantService,
            IOptions<IdentityApiOptions> options)
        {
            _userService = userService;
            _tenantService = tenantService;
            _options = options;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var users = await _userService.GetAsync();
            if (users?.Data == null)
            {
                return NotFound();
            }

            return Ok(users.Data);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateUserModel model)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var result = await _userService.CreateAsync(model);

            return result.Succeeded
                ? Created($"{_options.Value.RootPath ?? string.Empty}/users/{result.Data.Id}", result.Data)
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] TUser user)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var result = await _userService.UpdateAsync(user);
            if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
            {
                return NotFound();
            }

            return result.Succeeded ? Ok() : (IActionResult) BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var result = await _userService.DeleteAsync(id);
            if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
            {
                return NotFound();
            }

            return result.Succeeded ? Ok() : (IActionResult) BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        [HttpGet("id/{id}")]
        public async Task<IActionResult> FindById([FromRoute] string id)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            return user.Succeeded
                ? Ok(user.Data)
                : (IActionResult) BadRequest(user.Errors);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> FindByEmail([FromRoute] string email)
        {
            var user = await _userService.FindByEmailAsync(email);
            if (user?.Data == null)
            {
                return NotFound();
            }

            return user.Succeeded
                ? Ok(user.Data)
                : (IActionResult) BadRequest(user.Errors);
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> FindByUsername([FromRoute] string username)
        {
            var user = await _userService.FindByNameAsync(username);
            if (user?.Data == null)
            {
                return NotFound();
            }

            return user.Succeeded
                ? Ok(user.Data)
                : (IActionResult) BadRequest(user.Errors);
        }

        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> FindByPhoneNumber([FromRoute] string phone)
        {
            var user = await _userService.FindByPhoneNumberAsync(phone);
            if (user?.Data == null)
            {
                return NotFound();
            }

            return user.Succeeded
                ? Ok(user.Data)
                : (IActionResult) BadRequest(user.Errors);
        }

        #region Role Assignment

        [HttpGet("{id}/roles")]
        public async Task<IActionResult> GetRoles([FromRoute] string id)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            var result = await _userService.GetRolesAsync(user.Data);
            if (result.Data == null || result.Data.Count == 0)
            {
                return NotFound();
            }

            return Ok(result.Data);
        }

        [HttpPost("{id}/roles/{role}")]
        public async Task<IActionResult> AddToRole([FromRoute] string id, [FromRoute] string role)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            var result = await _userService.AddToRoleAsync(user.Data, role);

            return result.Succeeded
                ? Created($"/api/users/{user.Data}/roles", user)
                : (IActionResult) BadRequest(result.Errors);
        }


        [HttpDelete("{id}/roles/{role}")]
        public async Task<IActionResult> RemoveFromRole([FromRoute] string id, [FromRoute] string role)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            var result = await _userService.RemoveFromRoleAsync(user.Data, role);

            return result.Succeeded
                ? Ok()
                : (IActionResult) BadRequest(result.Errors);
        }

        #endregion

        #region Claim Assignment

        [HttpGet("{id}/claims")]
        public async Task<IActionResult> GetClaims([FromRoute] string id)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            var result = await _userService.GetClaimsAsync(user.Data);

            return result.Succeeded
                ? Ok(result.Data)
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpPost("{id}/claims")]
        public async Task<IActionResult> AddClaim([FromRoute] string id, [FromBody] AddClaimModel model)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            var claim = new Claim(model.Type, model.Value, model.ValueType ?? ClaimValueTypes.String);

            var result = await _userService.AddClaimAsync(user.Data, claim);

            return result.Succeeded
                ? Created($"/api/users/{user.Data}/claims", claim)
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpDelete("{id}/claims/{type}/{value}")]
        public async Task<IActionResult> RemoveClaim([FromRoute] string id, [FromRoute] string type,
            [FromRoute] string value)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user?.Data == null)
            {
                return NotFound();
            }

            var claims = await _userService.GetClaimsAsync(user.Data);

            var claim = claims.Data.FirstOrDefault(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                                                        x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

            if (claim == null)
            {
                return NotFound();
            }

            var result = await _userService.RemoveClaimAsync(user.Data, claim);

            return result.Succeeded
                ? StatusCode((int) HttpStatusCode.NoContent)
                : (IActionResult) BadRequest(result.Errors);
        }

        #endregion

        #region Tenant Assignment

        [HttpGet("email/{email}/tenants")]
        public async Task<IActionResult> FindTenantsByEmail([FromRoute] string email)
        {
            var tenants = await _tenantService.FindByEmailAsync(email);
            if (tenants?.Data == null)
            {
                return NotFound();
            }

            return tenants.Succeeded
                ? Ok(tenants.Data)
                : (IActionResult) BadRequest(tenants.Errors);
        }

        [HttpGet("username/{username}/tenants")]
        public async Task<IActionResult> FindTenantsByUsername([FromRoute] string username)
        {
            var tenants = await _tenantService.FindByUserNameAsync(username);
            if (tenants?.Data == null)
            {
                return NotFound();
            }

            return tenants.Succeeded
                ? Ok(tenants.Data)
                : (IActionResult) BadRequest(tenants.Errors);
        }

        [HttpGet("phone/{phone}/tenants")]
        public async Task<IActionResult> FindTenantsByPhoneNumber([FromRoute] string phone)
        {
            var tenants = await _tenantService.FindByPhoneNumberAsync(phone);
            if (tenants?.Data == null)
            {
                return NotFound();
            }

            return tenants.Succeeded
                ? Ok(tenants.Data)
                : (IActionResult) BadRequest(tenants.Errors);
        }

        #endregion
    }
}
