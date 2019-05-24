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
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.AspNetCore.Mvc.Controllers
{
    [Route("tenants")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Authorize(Constants.Security.Policies.ManageTenants)]
    [Category("Identity"), DisplayName("Tenants"), Description("Manages system tenants.")]
    public class TenantController<TTenant> : DataController where TTenant : IdentityTenant
    {
        private readonly ITenantService<TTenant> _tenantService;
        private readonly IOptions<IdentityApiOptions> _options;

        public TenantController(ITenantService<TTenant> tenantService, IOptions<IdentityApiOptions> options)
        {
            _tenantService = tenantService;
            _options = options;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var roles = await _tenantService.GetAsync();
            if (roles?.Data == null || roles.Data?.Count() == 0)
                return NotFound();
            return Ok(roles.Data);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateTenantModel model)
        {
            if (!ValidModelState(out var error))
                return error;

            var result = await _tenantService.CreateAsync(model);

            return result.Succeeded
                ? Created($"{_options.Value.RootPath ?? string.Empty}/tenants/{result.Data.Id}", result.Data)
                : (IActionResult)BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ValidModelState(out var error))
                return error;
            var result = await _tenantService.DeleteAsync(id);
            if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
                return NotFound();
            return result.Succeeded ? NoContent() : (IActionResult)BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] TTenant tenant)
        {
            if (!ValidModelState(out var error))
                return error;
            var result = await _tenantService.UpdateAsync(tenant);
            if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
                return NotFound();
            return result.Succeeded ? Ok() : (IActionResult)BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        [HttpGet("id/{id}")]
        public async Task<IActionResult> FindById([FromRoute] string id)
        {
            var user = await _tenantService.FindByIdAsync(id);
            if (user?.Data == null)
                return NotFound();
            return user.Succeeded
                ? Ok(user.Data)
                : (IActionResult)BadRequest(user.Errors);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> FindByUsername([FromRoute] string name)
        {
            var user = await _tenantService.FindByNameAsync(name);
            if (user?.Data == null)
                return NotFound();
            return user.Succeeded
                ? Ok(user.Data)
                : (IActionResult)BadRequest(user.Errors);
        }
    }
}
