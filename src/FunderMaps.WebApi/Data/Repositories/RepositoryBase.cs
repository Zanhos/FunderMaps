﻿using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Repositories;
using FunderMaps.Providers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FunderMaps.Data.Repositories
{
    /// <summary>
    /// Generic repository.
    /// </summary>
    /// <typeparam name="TEntry">Derivative of base entity.</typeparam>
    /// <typeparam name="TEntryPrimaryKey">Primary key of entity.</typeparam>
    public abstract class RepositoryBase<TEntry, TEntryPrimaryKey> : IAsyncRepository<TEntry, TEntryPrimaryKey>
        where TEntry : BaseEntity
    {
        /// <summary>
        /// Data provider interface.
        /// </summary>
        public DbProvider DataProvider { get; }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="dbProvider">Database provider.</param>
        protected RepositoryBase(DbProvider dbProvider) => DataProvider = dbProvider;

        /// <summary>
        /// Runs the SQL command and creates the connection if necessary.
        /// </summary>
        /// <typeparam name="TReturn">Query return type.</typeparam>
        /// <param name="action">SQL query.</param>
        /// <param name="connection">Optional database connection.</param>
        /// <returns>Return value.</returns>
        public async Task<TReturn> RunSqlCommand<TReturn>(Func<IDbConnection, Task<TReturn>> action, IDbConnection connection = null)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Run with existing connection.
            if (!(connection is null))
            {
                return await action(connection);
            }

            // Run in new scope.
            using var connectionScope = DataProvider.ConnectionScope();
            return await action(connectionScope);
        }

        /// <summary>
        /// <see cref="IAsyncRepository{TEntry, TEntryPrimaryKey}.GetByIdAsync"/>
        /// </summary>
        public abstract Task<TEntry> GetByIdAsync(TEntryPrimaryKey id);

        /// <summary>
        /// <see cref="IAsyncRepository{TEntry, TEntryPrimaryKey}.ListAllAsync"/>
        /// </summary>
        public abstract Task<IReadOnlyList<TEntry>> ListAllAsync(Navigation navigation);

        /// <summary>
        /// <see cref="IAsyncRepository{TEntry, TEntryPrimaryKey}.AddAsync"/>
        /// </summary>
        public abstract Task<TEntryPrimaryKey> AddAsync(TEntry entity);

        /// <summary>
        /// <see cref="IAsyncRepository{TEntry, TEntryPrimaryKey}.UpdateAsync"/>
        /// </summary>
        public abstract Task UpdateAsync(TEntry entity);

        /// <summary>
        /// <see cref="IAsyncRepository{TEntry, TEntryPrimaryKey}.DeleteAsync"/>
        /// </summary>
        public abstract Task DeleteAsync(TEntry entity);

        /// <summary>
        /// <see cref="IAsyncRepository{TEntry, TEntryPrimaryKey}.CountAsync"/>
        /// </summary>
        public abstract Task<uint> CountAsync();
    }
}