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
using System.Threading.Tasks;
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Data.Contracts.Mvc;
using HQ.Platform.Identity.Configuration;
using HQ.Platform.Identity.Models;
using HQ.Platform.Security.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HQ.Platform.Identity.AspNetCore.Mvc.Controllers
{
    [Route("applications")]
    [DynamicController]
    [DynamicAuthorize(typeof(IdentityApiOptions), nameof(IdentityApiOptions.Policies), nameof(IdentityApiOptions.Policies.Applications))]
	[ApiExplorerSettings(IgnoreApi = false)]
    [MetaCategory("Identity", "Manages application access controls.")]
    [DisplayName("Applications")]
    [MetaDescription("Manages system applications.")]
    public class ApplicationController<TApplication, TKey> : DataController
        where TApplication : IdentityApplication<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly IOptions<IdentityApiOptions> _options;
        private readonly IApplicationService<TApplication> _applicationService;

        public ApplicationController(IApplicationService<TApplication> applicationService, IOptions<IdentityApiOptions> options)
        {
            _applicationService = applicationService;
            _options = options;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            var applications = await _applicationService.GetAsync();
            if (applications?.Data == null || applications.Data?.Count() == 0)
            {
                return NotFound();
            }

            return Ok(applications.Data);
        }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody] CreateApplicationModel model)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var result = await _applicationService.CreateAsync(model);

            return result.Succeeded
                ? Created($"{_options.Value.RootPath ?? string.Empty}/applications/{result.Data.Id}", result.Data)
                : (IActionResult)BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var result = await _applicationService.DeleteAsync(id);
            if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
            {
                return NotFound();
            }

            return result.Succeeded ? NoContent() : (IActionResult)BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] TApplication tenant)
        {
            if (!ValidModelState(out var error))
            {
                return error;
            }

            var result = await _applicationService.UpdateAsync(tenant);
            if (!result.Succeeded && result.Errors.Count == 1 && result.Errors[0].StatusCode == 404)
            {
                return NotFound();
            }

            return result.Succeeded ? Ok() : (IActionResult)BadRequest(result.Errors);
        }

        [HttpGet("{id}")]
        [HttpGet("id/{id}")]
        public async Task<IActionResult> FindById([FromRoute] string id)
        {
            var application = await _applicationService.FindByIdAsync(id);
            if (application?.Data == null)
            {
                return NotFound();
            }

            return application.Succeeded
                ? Ok(application.Data)
                : (IActionResult)BadRequest(application.Errors);
        }

        [HttpGet("name/{name}")]
        public async Task<IActionResult> FindByUsername([FromRoute] string name)
        {
            var application = await _applicationService.FindByNameAsync(name);
            if (application?.Data == null)
            {
                return NotFound();
            }

            return application.Succeeded
                ? Ok(application.Data)
                : (IActionResult)BadRequest(application.Errors);
        }
    }
}
