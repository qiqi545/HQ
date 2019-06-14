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
using HQ.Common;
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Functions.AspNetCore.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TypeKitchen;
using Error = HQ.Data.Contracts.Error;

namespace HQ.Platform.Functions.AspNetCore.Mvc.Controllers
{
    [MetaCategory("Operations", "Provides diagnostic tools for server operators at runtime.")]
    [MetaDescription("Manages background tasks.")]
    [DisplayName("Background Tasks")]
    [Route("tasks")]
    [DynamicController]
    [ApiExplorerSettings(IgnoreApi = false)]
    [Authorize(Constants.Security.Policies.ManageBackgroundTasks)]
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
            var taskTypeNames = _typeResolver.FindByMethodName(nameof(Handler.Perform))
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

        [HttpPost("every/{minutes?}")]
        [MetaDescription("Creates a frequently repeating background task.")]
        public IActionResult CreateFrequentBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int minutes = 0)
        {
            model.Expression = CronTemplates.Minutely(minutes);

            return CreateBackgroundTask(model);
        }

        [HttpPost("daily/{atHour?}/{atMinute?}")]
        [MetaDescription("Creates a daily repeating background task.")]
        public IActionResult CreateDailyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int atHour = 0, [FromRoute] int atMinute = 0)
        {
            model.Expression = CronTemplates.Daily(atHour, atMinute);

            return CreateBackgroundTask(model);
        }

        [HttpPost("weekly/{dayOfWeek?}/{atHour?}/{atMinute?}")]
        [MetaDescription("Creates a weekly repeating background task.")]
        public IActionResult CreateWeeklyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] DayOfWeek dayOfWeek = DayOfWeek.Sunday, [FromRoute] int atHour = 0, [FromRoute] int atMinute = 0)
        {
            model.Expression = CronTemplates.WeekDaily(dayOfWeek, atHour, atMinute);

            return CreateBackgroundTask(model);
        }

        [HttpPost("monthly/{atDay?}/{atHour?}/{atMinute?}")]
        [MetaDescription("Creates a monthly repeating background task.")]
        public IActionResult CreateMonthlyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int atDay = 0, [FromRoute] int atHour = 0, [FromRoute] int atMinute = 0)
        {
            model.Expression = CronTemplates.Monthly(atDay, atHour, atMinute);

            return CreateBackgroundTask(model);
        }

        [HttpPost]
        [MetaDescription("Creates a background task.")]
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

                    if (!string.IsNullOrWhiteSpace(model.Expression))
                        t.Expression = model.Expression;
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