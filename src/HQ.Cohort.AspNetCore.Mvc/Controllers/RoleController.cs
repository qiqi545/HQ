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
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Cohort.Models;
using HQ.Common.AspNetCore;
using HQ.Tokens;
using HQ.Tokens.AspNetCore.Mvc.Attributes;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ClaimTypes = HQ.Tokens.ClaimTypes;

namespace HQ.Cohort.AspNetCore.Mvc.Controllers
{
    [Route("roles")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [RequireClaim(ClaimTypes.Permission, ClaimValues.ManageRoles)]
    public class RoleController<TRole> : Controller where TRole : IdentityRole
    {
        private readonly RoleManager<TRole> _roleManager;
        private readonly IOptions<SecurityOptions> _securityOptions;

        public RoleController(RoleManager<TRole> roleManager, IOptions<SecurityOptions> tokenOptions)
        {
            _roleManager = roleManager;
            _securityOptions = tokenOptions;
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles;
            // ReSharper disable once UseMethodAny.2
            if (roles.Count() == 0)
                return NotFound();
            return Ok(roles);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleModel model)
        {
            if (!this.TryValidateModelState(out var error))
                return error;

            var role = (TRole) FormatterServices.GetUninitializedObject(typeof(TRole));
            role.Name = model.Name;
            role.ConcurrencyStamp = model.ConcurrencyStamp ?? $"{Guid.NewGuid()}";

            var result = await _roleManager.CreateAsync(role);

            return result.Succeeded
                ? Created("/api/roles/" + role.Id, role)
                : (IActionResult) BadRequest(result.Errors);
        }


        [HttpGet("{id}/claims")]
        public async Task<IActionResult> GetClaims([FromRoute] string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var claims = await _roleManager.GetClaimsAsync(role);

            if (claims == null || claims.Count == 0)
                return NotFound();

            return Ok(claims);
        }

        [HttpPost("{id}/claims")]
        public async Task<IActionResult> AddClaim([FromRoute] string id, [FromBody] CreateClaimModel model)
        {
            if (!this.TryValidateModelState(out var error))
                return error;

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var issuer = _securityOptions.Value.Tokens.Issuer;
            var claim = new Claim(model.Type, model.Value, model.ValueType ?? ClaimValueTypes.String, issuer);
            var result = await _roleManager.AddClaimAsync(role, claim);

            return result.Succeeded
                ? Created("/api/roles/" + role.Id + "/claims", claim)
                : (IActionResult) BadRequest(result.Errors);
        }

        [HttpDelete("{id}/claims")]
        public async Task<IActionResult> RemoveClaim([FromRoute] string id, [FromQuery] DeleteClaimModel model)
        {
            if (!this.TryValidateModelState(out var error))
                return error;

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var issuer = _securityOptions.Value.Tokens.Issuer;
            var claim = new Claim(model.Type, model.Value, null, issuer);
            var result = await _roleManager.RemoveClaimAsync(role, claim);

            return result.Succeeded
                ? Created("/api/roles/" + role.Id + "/claims", claim)
                : (IActionResult) BadRequest(result.Errors);
        }


        [HttpGet("{id}")]
        public async Task<TRole> FindById([FromRoute] string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            return role;
        }
    }
}
