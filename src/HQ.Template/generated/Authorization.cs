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
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services, IConfiguration securityConfig)
        {
            return services.AddAuthorizationPolicies(securityConfig.Bind);
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services, Action<SecurityOptions> configureAction = null)
        {
            var options = new SecurityOptions();
            configureAction?.Invoke(options);

            services.AddAuthorization(x =>
            {
                x.AddPolicy(Policies.CreatePeople, p => 
                {
                    p.RequireClaimExtended(services, options, options.Claims.PermissionClaim, Permissions.CreatePeople);
                });
                x.AddPolicy(Policies.RetrievePeople, p => 
                {
                    p.RequireClaimExtended(services, options, options.Claims.PermissionClaim, Permissions.RetrievePeople);
                });
                x.AddPolicy(Policies.UpdatePeople, p => 
                {
                    p.RequireClaimExtended(services, options, options.Claims.PermissionClaim, Permissions.UpdatePeople);
                });
                x.AddPolicy(Policies.DeletePeople, p => 
                {
                    p.RequireClaimExtended(services, options, options.Claims.PermissionClaim, Permissions.DeletePeople);
                });
            });
            return services;
        }
    }
}
namespace HQ.Template
{
    public static partial class Policies
    {
        public const string CreatePeople = "CreatePeople";
        public const string RetrievePeople = "RetrievePeople";
        public const string UpdatePeople = "UpdatePeople";
        public const string DeletePeople = "DeletePeople";
    }

    public static partial class Permissions
    {
        public const string CreatePeople = "create_people";
        public const string RetrievePeople = "retrieve_people";
        public const string UpdatePeople = "update_people";
        public const string DeletePeople = "delete_people";
    }

}
