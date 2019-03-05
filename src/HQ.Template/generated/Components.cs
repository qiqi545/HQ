/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Dapper;
using Morcatko.AspNetCore.JsonMergePatch;
using ErrorStrings = HQ.Data.Contracts.ErrorStrings;
using HQ.Common.FastMember;
using HQ.Common.Models;
using HQ.Common;
using HQ.Data.Contracts.AspNetCore.Mvc;
using HQ.Data.Contracts.Configuration;
using HQ.Data.Contracts;
using HQ.Data.SessionManagement;
using HQ.Data.Sql.Batching;
using HQ.Data.Sql.Builders;
using HQ.Data.Sql.Descriptor;
using HQ.Data.Sql.Dialects;
using HQ.Data.Sql.Queries.Rosetta;
using HQ.Data.Sql.Queries;
using HQ.Data.Streaming.Fields;
using HQ.Data.Streaming;
using HQ.DotLiquid;
using HQ.Extensions.CodeGeneration.Scripting;
using HQ.Extensions.Metrics;
using HQ.Platform.Api.Configuration;
using HQ.Platform.Api.Conventions;
using HQ.Platform.Api.Extensions;
using HQ.Platform.Api.Filters;
using HQ.Platform.Api.Models;
using HQ.Platform.Runtime.Rest.Attributes;
using HQ.Platform.Security.AspNetCore.Extensions;
using HQ.Platform.Security.Configuration;


namespace HQ.Template
{

    public class GeneratedComponent : IDynamicComponent
    {
        public IEnumerable<Type> ControllerTypes => new[]
        {
            typeof(PersonController),
        };

         public Func<string> Namespace { get; internal set; }
    }

}
