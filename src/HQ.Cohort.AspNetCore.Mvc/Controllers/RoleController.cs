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
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using HQ.Cohort.Models;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Rosetta.AspNetCore.Mvc;
using HQ.Tokens.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Cohort.AspNetCore.Mvc.Controllers
{
    [Route("roles")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Constants.Security.Policies.ManageUsers)]
    public class RoleController<TRole> : DataController where TRole : IdentityRole
    {
        private readonly IRoleService<TRole> _roleService;
        private readonly IOptions<SecurityOptions> _security;

        public RoleController(IRoleService<TRole> roleService, IOptions<SecurityOptions> security)
        {
            _roleService = roleService;
            _security = security;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var roles = await _roleService.GetAsync();
            if (roles?.Data == null)
                return NotFound();
            return Ok(roles.Data);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateRoleModel model)
        {
            if (!ValidModelState(out var error))
                return error;

            var result = await _roleService.CreateAsync(model);

            return result.Succeeded
                ? Created("/api/roles/" + result.Data.Id, result.Data)
                : (IActionResult)BadRequest(result.Errors);
        }


        [HttpGet("{id}/claims")]
        public async Task<IActionResult> GetClaims([FromRoute] string id)
        {
            var role = await _roleService.FindByIdAsync(id);
            if (role?.Data == null)
                return NotFound();

            var claims = await _roleService.GetClaimsAsync(role.Data);

            if (claims?.Data.Count == 0)
                return NotFound();

            return Ok(claims);
        }

        [HttpPost("{id}/claims")]
        public async Task<IActionResult> AddClaim([FromRoute] string id, [FromBody] CreateClaimModel model)
        {
            if (!Valid(model, out var error))
                return error;

            var role = await _roleService.FindByIdAsync(id);
            if (role?.Data == null)
                return NotFound();

            var issuer = _security.Value.Tokens.Issuer;
            var claim = new Claim(model.Type, model.Value, model.ValueType ?? ClaimValueTypes.String, issuer);
            var result = await _roleService.AddClaimAsync(role.Data, claim);

            return result.Succeeded
                ? Created($"/api/roles/{role.Data.Id}/claims", claim)
                : (IActionResult)BadRequest(result.Errors);
        }

        [HttpDelete("{id}/claims/{type}/{value}")]
        public async Task<IActionResult> RemoveClaim([FromRoute] string id, [FromRoute] string type, [FromRoute] string value)
        {
            var user = await _roleService.FindByIdAsync(id);
            if (user?.Data == null)
                return NotFound();

            var claims = await _roleService.GetClaimsAsync(user.Data);

            var claim = claims.Data.FirstOrDefault(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                                                        x.Value.Equals(value, StringComparison.OrdinalIgnoreCase));

            if (claim == null)
                return NotFound();

            var result = await _roleService.RemoveClaimAsync(user.Data, claim);

            return result.Succeeded
                ? StatusCode((int)HttpStatusCode.NoContent)
                : (IActionResult)BadRequest(result.Errors);
        }


        [HttpGet("{id}")]
        public async Task<TRole> FindById([FromRoute] string id)
        {
            var role = await _roleService.FindByIdAsync(id);
            return role.Data;
        }
    }
}
