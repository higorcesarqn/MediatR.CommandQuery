﻿using System;
using System.Collections.Generic;
using EntityFrameworkCore.CommandQuery.SqlServer.Tests.Data;
using EntityFrameworkCore.CommandQuery.SqlServer.Tests.Domain.Models;
using KickStart.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace EntityFrameworkCore.CommandQuery.SqlServer.Tests.Domain
{
    public class RoleServiceRegistration : IDependencyInjectionRegistration
    {
        public void Register(IServiceCollection services, IDictionary<string, object> data)
        {
            services.AddEntityQueries<TrackerContext, Data.Entities.Role, Guid, RoleReadModel>();
            services.AddEntityCommands<TrackerContext, Data.Entities.Role, Guid, RoleReadModel, RoleCreateModel, RoleUpdateModel>();
        }
    }
}