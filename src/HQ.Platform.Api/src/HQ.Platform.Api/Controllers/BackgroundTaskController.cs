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
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Api.Attributes;
using HQ.Platform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TypeKitchen;

namespace HQ.Platform.Api.Controllers
{
    public class HelloWorld
    {
        public bool Perform()
        {
            Console.WriteLine("Hello, World!");
            return true;
        }
    }

    [Route("ops/tasks")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    [MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
    [MetaDescription("Manages background tasks.")]
    public class BackgroundTaskController : DataController
    {
        private readonly BackgroundTaskHost _host;

        private readonly IBackgroundTaskStore _store;
        private readonly ITypeResolver _typeResolver;
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
            var taskTypeNames = _typeResolver.FindByMethodName(nameof(HQ.Extensions.Scheduling.Hooks.Handler.Perform))
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Select(x => x.Name);

            return Ok(new { Options = _options.CurrentValue, TaskTypes = taskTypeNames });
        }

        [HttpGet]
        public IActionResult GetBackgroundTasks()
        {
            return Ok(_store.GetAll());
        }

        [HttpGet, MustHaveQueryParameters("tags")]
        public IActionResult GetBackgroundTasksByTag([FromQuery] string tags)
        {
            return Ok(_store.GetByAnyTags(tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)));
        }

        [HttpGet("{id}")]
        public IActionResult GetBackgroundTaskById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var taskId))
                return Error(new Error(ErrorEvents.InvalidRequest, "Invalid task ID"));

            var task = _store.GetById(taskId);
            if (task == null)
                return NotFoundError(ErrorEvents.ResourceMissing, "No task found with ID {0}", id);

            return Ok(task);
        }

        [HttpPost]
        public IActionResult CreateBackgroundTask([FromBody] CreateBackgroundTaskModel model)
        {
            if (!Valid(model, out var error))
                return error;

            if (!string.IsNullOrWhiteSpace(model.TaskType))
            {
                var type = _typeResolver.FindByFullName(model.TaskType) ?? _typeResolver.FindFirstByName(model.TaskType);
                if (type == null)
                    return BadRequestError(ErrorEvents.ResourceMissing, "No task type found with name {0}", model.TaskType);

                if (!_host.TryScheduleTask(type, out var task, t =>
                {
                    if (model.Tags?.Length > 0)
                        t.Tags.AddRange(model.Tags);
                }))
                {
                    return NotAcceptableError(ErrorEvents.CouldNotAcceptWork, "Task was not accepted by the server");
                }

                return Accepted(new Uri($"{Request.Path}/{task.Id}", UriKind.Relative));
            }

            return NotImplemented();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBackgroundTask(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var taskId))
                return Error(new Error(ErrorEvents.InvalidRequest, "Invalid task ID"));

            var task = _store.GetById(taskId);
            if (task == null)
                return NotFound();

            _store.Delete(task);
            return NoContent();
        }
    }
}
