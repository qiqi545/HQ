using System;
using System.Collections.Generic;
using HQ.Common.AspNetCore.Mvc;
using HQ.Platform.Operations.Controllers;

namespace HQ.Platform.Operations.Models
{
    internal class ConfigurationComponent : IDynamicComponent
    {
        public IEnumerable<Type> ControllerTypes => new[]
        {
            typeof(ConfigurationController)
        };

        public Func<string> Namespace { get; set; }
    }
}
