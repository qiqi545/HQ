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
using HQ.Common.AspNetCore.Mvc;
using HQ.Data.Contracts;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Attributes;
using HQ.Extensions.Scheduling.Configuration;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Models;
using HQ.Platform.Functions.AspNetCore.Mvc.Models;
using HQ.Platform.Security.AspNetCore;
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
    [ApiExplorerSettings(IgnoreApi = false)]
	[DynamicController]
    [DynamicAuthorize(typeof(BackgroundTaskOptions))]
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
            var taskTypeNames = _typeResolver.FindByMethodName(nameof(Handler.PerformAsync))
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .Select(x => x.Name);

            return Ok(new { Options = _options.CurrentValue, TaskTypes = taskTypeNames });
        }

        [HttpGet]
        public async Task<IActionResult> GetBackgroundTasks()
        {
            return Ok(await _store.GetAllAsync());
        }

        [HttpGet, MustHaveQueryParameters("tags")]
        public async Task<IActionResult> GetBackgroundTasksByTag([FromQuery] string tags)
        {
            return Ok(await _store.GetByAnyTagsAsync(tags.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBackgroundTaskById(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var taskId))
                return Error(new Error(ErrorEvents.InvalidRequest, "Invalid task ID"));

            var task = await _store.GetByIdAsync(taskId);
            if (task == null)
                return NotFoundError(ErrorEvents.ResourceMissing, "No task found with ID {0}", id);

            return Ok(task);
        }

        [HttpPost("secondly/{seconds?}")]
        [MetaDescription("Creates a frequently repeating background task, occurring on a schedule every N second(s).")]
        public async Task<IActionResult> CreateSecondlyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int seconds = 0)
        {
            model.Expression = CronTemplates.Secondly(seconds);

            return await CreateBackgroundTask(model);
        }

        [HttpPost("minutely/{minutes?}/{atSecond?}")]
        [MetaDescription("Creates a frequently repeating background task, occurring on a schedule every N minute(s).")]
        public async Task<IActionResult> CreateMinutelyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int minutes = 0, [FromRoute] int atSecond = 0)
        {
            model.Expression = CronTemplates.Minutely(minutes, atSecond);

            return await CreateBackgroundTask(model);
        }

        [HttpPost("daily/{atHour?}/{atMinute?}/{atSecond?}")]
        [MetaDescription("Creates a daily repeating background task.")]
        public async Task<IActionResult> CreateDailyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int atHour = 0, [FromRoute] int atMinute = 0, [FromRoute] int atSecond = 0)
        {
            model.Expression = CronTemplates.Daily(atHour, atMinute, atSecond);

            return await CreateBackgroundTask(model);
        }

        [HttpPost("weekly/{dayOfWeek?}/{atHour?}/{atMinute?}/{atSecond?}")]
        [MetaDescription("Creates a weekly repeating background task.")]
        public async Task<IActionResult> CreateWeeklyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] DayOfWeek dayOfWeek = DayOfWeek.Sunday, [FromRoute] int atHour = 0, [FromRoute] int atMinute = 0, [FromRoute] int atSecond = 0)
        {
            model.Expression = CronTemplates.Weekly(dayOfWeek, atHour, atMinute, atSecond);

            return await CreateBackgroundTask(model);
        }

        [HttpPost("monthly/{atDay?}/{atHour?}/{atMinute?}/{atSecond?}")]
        [MetaDescription("Creates a monthly repeating background task.")]
        public async Task<IActionResult> CreateMonthlyBackgroundTask([FromBody] CreateBackgroundTaskModel model, [FromRoute] int atDay = 0, [FromRoute] int atHour = 0, [FromRoute] int atMinute = 0, [FromRoute] int atSecond = 0)
        {
            model.Expression = CronTemplates.Monthly(atDay, atHour, atMinute, atSecond);

            return await CreateBackgroundTask(model);
        }

        [HttpPost]
        [MetaDescription("Creates a background task.")]
        public async Task<IActionResult> CreateBackgroundTask([FromBody] CreateBackgroundTaskModel model)
        {
            if (!Valid(model, out var error))
                return error;

            if (!string.IsNullOrWhiteSpace(model.TaskType))
            {
                var type = _typeResolver.FindByFullName(model.TaskType) ?? _typeResolver.FindFirstByName(model.TaskType);
                if (type == null)
                    return BadRequestError(ErrorEvents.ResourceMissing, "No task type found with name {0}", model.TaskType);

                var result = await _host.TryScheduleTaskAsync(type, t =>
                {
                    if (model.Tags?.Length > 0)
                        t.Tags.AddRange(model.Tags);

                    if (!string.IsNullOrWhiteSpace(model.Expression))
                        t.Expression = model.Expression;
                });

                if (!result.Item1)
                {
                    return NotAcceptableError(ErrorEvents.CouldNotAcceptWork, "Task was not accepted by the server");
                }

                return Accepted(new Uri($"{Request.Path}/{result.Item2.Id}", UriKind.Relative));
            }

            return NotImplemented();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBackgroundTask(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out var taskId))
                return Error(new Error(ErrorEvents.InvalidRequest, "Invalid task ID"));

            var task = await _store.GetByIdAsync(taskId);
            if (task == null)
                return NotFound();

            await _store.DeleteAsync(task);
            return NoContent();
        }
    }
}
