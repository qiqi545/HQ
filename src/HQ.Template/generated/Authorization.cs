/*
	This code was generated by a tool. (c) 2019 HQ.IO Corporation. All rights reserved.
*/

using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
