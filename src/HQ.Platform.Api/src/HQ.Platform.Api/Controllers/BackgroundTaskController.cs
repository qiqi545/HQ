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
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Api.Attributes;
using HQ.Platform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HQ.Platform.Api.Controllers
{
    [Route("ops/tasks")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    [MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
    [MetaDescription("Manages background tasks.")]
    public class BackgroundTaskController : DataController
    {
        private readonly IBackgroundTaskStore _store;
        private readonly ITypeResolver _typeResolver;
        private readonly BackgroundTaskHost _host;
        private readonly IOptionsMonitor<BackgroundTaskOptions> _options;

        public BackgroundTaskController(IBackgroundTaskStore store, ITypeResolver typeResolver, BackgroundTaskHost host, IOptionsMonitor<BackgroundTaskOptions> options)
        {
            _store = store;
            _typeResolver = typeResolver;
            _host = host;
            _options = options;
        }

        [HttpOptions]
        public IActionResult GetOptions()
        {
            return Ok(_options.CurrentValue);
        }

        [HttpGet]
        public IActionResult GetBackgroundTasks()
        {
            return Ok(_store.GetAll());
        }

        [HttpGet, MustHaveQueryParameters]
        public IActionResult GetBackgroundTasksByTag([FromQuery] StringValues tags)
        {
            return Ok(_store.GetByAnyTags(tags));
        }

        [HttpGet("{id}")]
        public IActionResult GetBackgroundTaskById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var taskId))
                return Error(new Error(ErrorEvents.ValidationFailed, "Invalid task ID."));

            var task = _store.GetById(taskId);
            if (task == null)
                return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public IActionResult CreateBackgroundTask(CreateBackgroundTaskModel model)
        {
            if (!Valid(model, out var error))
                return error;

            var type = _typeResolver.FindByName(model.TaskType);
            if (type == null)
                return BadRequest(new { Message = "Unrecognized task type."});
            
            if (!_host.TryScheduleTask(type, out var task))
                return NotAcceptable();

            return Accepted(new Uri($"{Request.Path}/{task.Id}", UriKind.Relative));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBackgroundTask(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var taskId))
                return Error(new Error(ErrorEvents.ValidationFailed, "Invalid task ID."));

            var task = _store.GetById(taskId);
            if (task == null)
                return NotFound();

            _store.Delete(task);
            return NoContent();
        }
    }
}
