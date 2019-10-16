using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HQ.Extensions.Options;
using HQ.Platform.Api.Functions.AspNetCore.Mvc.Models;
using HQ.Test.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Platform.Tests.Api.Functions
{
	public class CreateBackgroundTaskModelTests : UnitUnderTest
	{
		[Test]
		public void Can_validate()
		{
			var model = new CreateBackgroundTaskModel();
			model.TaskCode = null;
			model.TaskType = null;

			var serviceCollection = new ServiceCollection();
			var serviceProvider = serviceCollection.BuildServiceProvider();

			var context = new ValidationContext(model, serviceProvider, null);
			var results = new List<ValidationResult>();
			Validator.TryValidateObject(model, context, results, true);
		}
	}
}
