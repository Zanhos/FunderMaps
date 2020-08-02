﻿using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunderMaps.IntegrationTests.Repositories
{
    public abstract class TestRepositoryBase<TEntity, TEntryPrimaryKey> : IAsyncRepository<TEntity, TEntryPrimaryKey>
        where TEntity : BaseEntity
        where TEntryPrimaryKey : IEquatable<TEntryPrimaryKey>
    {
        protected Func<TEntity, TEntryPrimaryKey> EntityPrimaryKey { get; }

        public EntityDataStore<TEntity> DataStore { get; set; }

        public TestRepositoryBase(EntityDataStore<TEntity> dataStore, Func<TEntity, TEntryPrimaryKey> entryPrimaryKey)
        {
            DataStore = dataStore;
            EntityPrimaryKey = entryPrimaryKey;
        }

        public virtual ValueTask<TEntryPrimaryKey> AddAsync(TEntity entity)
        {
            DataStore.Add(entity);
            return new ValueTask<TEntryPrimaryKey>(EntityPrimaryKey(entity));
        }

        public virtual ValueTask<ulong> CountAsync()
        {
            return new ValueTask<ulong>(DataStore.Count());
        }

        public virtual ValueTask DeleteAsync(TEntryPrimaryKey id)
        {
            DataStore.Entities.Remove(DataStore.Entities.First(e => EntityPrimaryKey(e).Equals(id)));
            return new ValueTask();
        }

        public virtual ValueTask<TEntity> GetByIdAsync(TEntryPrimaryKey id)
        {
            return new ValueTask<TEntity>(DataStore.Entities.FirstOrDefault(e => EntityPrimaryKey(e).Equals(id)));
        }

        public virtual IAsyncEnumerable<TEntity> ListAllAsync(INavigation navigation)
        {
            return Helper.AsAsyncEnumerable(Helper.ApplyNavigation(DataStore.Entities, navigation));
        }

        public abstract ValueTask UpdateAsync(TEntity entity);
    }
}
