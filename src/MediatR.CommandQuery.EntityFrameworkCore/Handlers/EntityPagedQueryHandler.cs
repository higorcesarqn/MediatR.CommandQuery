﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR.CommandQuery.Extensions;
using MediatR.CommandQuery.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediatR.CommandQuery.EntityFrameworkCore.Handlers
{
    public class EntityPagedQueryHandler<TContext, TEntity, TReadModel>
        : DataContextHandlerBase<TContext, EntityPagedQuery<TReadModel>, EntityPagedResult<TReadModel>>
        where TEntity : class
        where TContext : DbContext
    {

        public EntityPagedQueryHandler(ILoggerFactory loggerFactory, TContext dataContext, IMapper mapper)
            : base(loggerFactory, dataContext, mapper)
        {
        }


        protected override async Task<EntityPagedResult<TReadModel>> Process(EntityPagedQuery<TReadModel> request, CancellationToken cancellationToken)
        {
            var query = DataContext
                .Set<TEntity>()
                .AsNoTracking();

            // build query from filter
            query = BuildQuery(request, query);

            // get total for query
            int total = await QueryTotal(request, query, cancellationToken)
                .ConfigureAwait(false);

            // short circuit if total is zero
            if (total == 0)
                return new EntityPagedResult<TReadModel> { Data = new List<TReadModel>() };

            // page the query and convert to read model
            var result = await QueryPageed(request, query, cancellationToken)
                .ConfigureAwait(false);

            return new EntityPagedResult<TReadModel>
            {
                Total = total,
                Data = result
            };
        }


        protected virtual IQueryable<TEntity> BuildQuery(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query)
        {
            var entityQuery = request.Query;

            // build query from filter
            if (entityQuery?.Filter != null)
                query = query.Filter(entityQuery.Filter);

            // add raw query
            if (!string.IsNullOrEmpty(entityQuery.Query))
                query = query.Where(entityQuery.Query);


            return query;
        }

        protected virtual async Task<int> QueryTotal(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query, CancellationToken cancellationToken)
        {
            return await query
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        protected virtual async Task<IReadOnlyCollection<TReadModel>> QueryPageed(EntityPagedQuery<TReadModel> request, IQueryable<TEntity> query, CancellationToken cancellationToken)
        {
            var entityQuery = request.Query;

            return await query
                .Sort(entityQuery.Sort)
                .Page(entityQuery.Page, entityQuery.PageSize)
                .ProjectTo<TReadModel>(Mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}