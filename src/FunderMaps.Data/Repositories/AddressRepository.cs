﻿using FunderMaps.Core.Entities;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Interfaces.Repositories;
using FunderMaps.Core.Types;
using FunderMaps.Data.Extensions;
using FunderMaps.Data.Providers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

#pragma warning disable CA1812 // Internal class is never instantiated
namespace FunderMaps.Data.Repositories
{
    /// <summary>
    ///     Address repository.
    /// </summary>
    internal class AddressRepository : RepositoryBase<Address, string>, IAddressRepository
    {
        /// <summary>
        ///     Create new <see cref="Address"/>.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        /// <returns>Created <see cref="Address"/>.</returns>
        public override async ValueTask<string> AddAsync(Address entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var sql = @"
                    INSERT INTO geocoder.address(
                        building_number,
                        postal_code,
                        street,
                        is_active,
                        external_id,
                        external_source)
                    VALUES (
                        @building_number,
                        @postal_code,
                        @street,
                        @is_active,
                        @external_id,
                        @external_source)
                    ON CONFLICT DO NOTHING
                    RETURNING id";

            await using var connection = await DbProvider.OpenConnectionScopeAsync(AppContext.CancellationToken);
            await using var cmd = DbProvider.CreateCommand(sql, connection);

            MapToWriter(cmd, entity);

            await using var reader = await cmd.ExecuteReaderAsyncEnsureRowAsync(AppContext.CancellationToken);
            await reader.ReadAsync(AppContext.CancellationToken);

            return reader.GetSafeString(0);
        }

        /// <summary>
        ///     Retrieve number of entities.
        /// </summary>
        /// <returns>Number of entities.</returns>
        public override ValueTask<ulong> CountAsync()
        {
            var sql = @"
                SELECT  COUNT(*)
                FROM    geocoder.address";

            return ExecuteScalarUnsignedLongCommandAsync(sql);
        }

        /// <summary>
        ///     Delete <see cref="Incident"/>.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        public override ValueTask DeleteAsync(string id)
            => throw new InvalidOperationException();

        private static void MapToWriter(DbCommand cmd, Address entity)
        {
            cmd.AddParameterWithValue("building_number", entity.BuildingNumber);
            cmd.AddParameterWithValue("postal_code", entity.PostalCode);
            cmd.AddParameterWithValue("street", entity.Street);
            cmd.AddParameterWithValue("is_active", entity.IsActive);
            cmd.AddParameterWithValue("external_id", entity.ExternalId);
            cmd.AddParameterWithValue("external_source", entity.ExternalSource);
        }

        private static Address MapFromReader(DbDataReader reader)
            => new Address
            {
                Id = reader.GetSafeString(0),
                BuildingNumber = reader.GetSafeString(1),
                PostalCode = reader.GetSafeString(2),
                Street = reader.GetSafeString(3),
                IsActive = reader.GetBoolean(4),
                ExternalId = reader.GetSafeString(5),
                ExternalSource = reader.GetFieldValue<ExternalDataSource>(6),
                City = reader.GetSafeString(7),
                BuildingId = reader.GetSafeString(8),
                BuildingNavigation = new Building // TODO: Remove in future
                {
                    Id = reader.GetSafeString(9),
                    BuildingType = reader.GetFieldValue<BuildingType?>(10),
                    BuiltYear = reader.GetDateTime(11),
                    IsActive = reader.GetBoolean(12),
                    ExternalId = reader.GetSafeString(13),
                    ExternalSource = reader.GetFieldValue<ExternalDataSource>(14),
                    Geometry = reader.GetString(15),
                    NeighborhoodId = reader.GetSafeString(16),
                }
            };

        public async ValueTask<Address> GetByExternalIdAsync(string id, ExternalDataSource source)
        {
            var sql = @"
                SELECT  -- Address
                        a.id,
                        a.building_number,
                        a.postal_code,
                        a.street,
                        a.is_active,
                        a.external_id,
                        a.external_source,
                        a.city,
                        a.building_id,

                        -- Building
                        b.id,
                        b.building_type,
                        b.built_year,
                        b.is_active,
                        b.external_id, 
                        b.external_source, 
                        b.geom,
                        b.neighborhood_id
                FROM    geocoder.address
                WHERE   external_id = @external_id
                AND     external_source = @external_source
                LIMIT   1";

            await using var connection = await DbProvider.OpenConnectionScopeAsync(AppContext.CancellationToken);
            await using var cmd = DbProvider.CreateCommand(sql, connection);

            cmd.AddParameterWithValue("external_id", id);
            cmd.AddParameterWithValue("external_source", source);

            await using var reader = await cmd.ExecuteReaderAsyncEnsureRowAsync(AppContext.CancellationToken);
            await reader.ReadAsync(AppContext.CancellationToken);

            return MapFromReader(reader);
        }

        /// <summary>
        ///     Retrieve <see cref="Address"/> by id.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <returns><see cref="Address"/>.</returns>
        public override async ValueTask<Address> GetByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- Address
                        a.id,
                        a.building_number,
                        a.postal_code,
                        a.street,
                        a.is_active,
                        a.external_id,
                        a.external_source,
                        a.city,
                        a.building_id,

                        -- Building
                        b.id,
                        b.building_type,
                        b.built_year,
                        b.is_active,
                        b.external_id, 
                        b.external_source, 
                        b.geom,
                        b.neighborhood_id
                FROM    geocoder.address
                WHERE   id = @id
                LIMIT   1";

            await using var connection = await DbProvider.OpenConnectionScopeAsync(AppContext.CancellationToken);
            await using var cmd = DbProvider.CreateCommand(sql, connection);

            cmd.AddParameterWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsyncEnsureRowAsync(AppContext.CancellationToken);
            await reader.ReadAsync(AppContext.CancellationToken);

            return MapFromReader(reader);
        }

        /// <summary>
        ///     Retrieve <see cref="Address"/> by search query.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns><see cref="Address"/>.</returns>
        public async IAsyncEnumerable<Address> GetBySearchQueryAsync(string query, INavigation navigation)
        {
            if (navigation == null)
            {
                throw new ArgumentNullException(nameof(navigation));
            }

            var sql = @"
                SELECT  -- Address
                        a.id,
                        a.building_number,
                        a.postal_code,
                        a.street,
                        a.is_active,
                        a.external_id,
                        a.external_source,
                        a.city,
                        a.building_id,

                        -- Building
                        b.id,
                        b.building_type,
                        b.built_year,
                        b.is_active,
                        b.external_id, 
                        b.external_source, 
                        b.geom,
                        b.neighborhood_id
                FROM    geocoder.search_address(@query) AS a
                JOIN    geocoder.building_encoded_geom AS b ON b.id = a.building_id";

            ConstructNavigation(ref sql, navigation);

            await using var connection = await DbProvider.OpenConnectionScopeAsync(AppContext.CancellationToken);
            await using var cmd = DbProvider.CreateCommand(sql, connection);

            cmd.AddParameterWithValue("query", query);

            await using var reader = await cmd.ExecuteReaderAsync(AppContext.CancellationToken);
            while (await reader.ReadAsync(AppContext.CancellationToken))
            {
                yield return MapFromReader(reader);
            }
        }

        /// <summary>
        ///     Retrieve all <see cref="Address"/>.
        /// </summary>
        /// <returns>List of <see cref="Address"/>.</returns>
        public override async IAsyncEnumerable<Address> ListAllAsync(INavigation navigation)
        {
            if (navigation == null)
            {
                throw new ArgumentNullException(nameof(navigation));
            }

            var sql = @"
                SELECT  -- Address
                        a.id,
                        a.building_number,
                        a.postal_code,
                        a.street,
                        a.is_active,
                        a.external_id,
                        a.external_source,
                        a.city,
                        a.building_id,

                        -- Building
                        b.id,
                        b.building_type,
                        b.built_year,
                        b.is_active,
                        b.external_id, 
                        b.external_source, 
                        b.geom,
                        b.neighborhood_id
                FROM    geocoder.address";

            ConstructNavigation(ref sql, navigation);

            await using var connection = await DbProvider.OpenConnectionScopeAsync(AppContext.CancellationToken);
            await using var cmd = DbProvider.CreateCommand(sql, connection);

            await using var reader = await cmd.ExecuteReaderCanHaveZeroRowsAsync(AppContext.CancellationToken);
            while (await reader.ReadAsync(AppContext.CancellationToken))
            {
                yield return MapFromReader(reader);
            }
        }

        /// <summary>
        ///     Update <see cref="Address"/>.
        /// </summary>
        /// <param name="entity">Entity object.</param>
        public override async ValueTask UpdateAsync(Address entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var sql = @"
                    UPDATE  geocoder.address
                    SET     building_number = @building_number,
                            postal_code = @postal_code,
                            street = @street,
                            is_active = @is_active,
                            external_id = @external_id,
                            external_source = @external_source
                    WHERE   id = @id";

            using var connection = await DbProvider.OpenConnectionScopeAsync(AppContext.CancellationToken);
            using var cmd = DbProvider.CreateCommand(sql, connection);

            cmd.AddParameterWithValue("id", entity.Id);

            MapToWriter(cmd, entity);

            await cmd.ExecuteNonQueryEnsureAffectedAsync(AppContext.CancellationToken);
        }
    }
}
#pragma warning restore CA1812 // Internal class is never instantiated
