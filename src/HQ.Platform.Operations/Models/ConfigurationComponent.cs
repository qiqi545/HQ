using System;
using System.Collections.Generic;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Operations.Controllers;

namespace HQ.Platform.Operations.Models
{
    internal class ConfigurationComponent : DynamicComponent
    {
        public override IEnumerable<Type> ControllerTypes => new[]
        {
            typeof(ConfigurationController)
        };
    }
}
