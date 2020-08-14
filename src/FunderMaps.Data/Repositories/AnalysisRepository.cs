﻿using FunderMaps.Core.Exceptions;
using FunderMaps.Core.Extensions;
using FunderMaps.Core.Helpers;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Interfaces.Repositories;
using FunderMaps.Core.Types;
using FunderMaps.Core.Types.Products;
using FunderMaps.Data.Extensions;
using FunderMaps.Data.Providers;
using FunderMaps.Webservice.Utility;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable CA1812 // Internal class is never instantiated
namespace FunderMaps.Data.Repositories
{
    /// <summary>
    ///     Repository for analysis products.
    /// </summary>
    internal sealed class AnalysisRepository : DataBase, IAnalysisRepository
    {
        private readonly IDescriptionService _descriptionService;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public AnalysisRepository(DbProvider dbProvider,
            IDescriptionService descriptionService)
            : base(dbProvider)
        {
            _descriptionService = descriptionService ?? throw new ArgumentNullException(nameof(descriptionService));
        }

        /// <summary>
        ///     Scrapped for now.
        /// </summary>
        public Task<IEnumerable<AnalysisProduct>> GetAllInFenceAsync(Guid userId, CancellationToken token) => throw new NotImplementedException();

        /// <summary>
        ///     Gets an analysis product by its external building id and source.
        /// </summary>
        /// <remarks>
        ///     If the building is outside the geofence, an <see cref="EntityNotFoundException"/>
        ///     is thrown. Check this condition before calling this function.
        /// </remarks>
        /// <param name="userId">Internal user id.</param>
        /// <param name="externalId">External building id.</param>
        /// <param name="externalSource">External data source</param>
        /// <param name="token"><see cref="CancellationToken"/></param>
        /// <returns><see cref="AnalysisProduct"/></returns>
        public async Task<AnalysisProduct> GetByExternalIdAsync(Guid userId, string externalId, ExternalDataSource externalSource, CancellationToken token)
        {
            // Validate parameters.
            userId.ThrowIfNullOrEmpty();
            externalId.ThrowIfNullOrEmpty();
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            // Build sql.
            var sql = @"
                SELECT 
                    * 
                FROM geocoder.analysis_complete AS ac
                WHERE 
                    ac.external_id = @ExternalId
                    AND
                    ac.external_source = @ExternalSource";

            // Execute sql.
            await using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            await using var cmd = DbProvider.CreateCommand(sql, connection);
            cmd.AddParameterWithValue("ExternalId", externalId);
            cmd.AddParameterWithValue("ExternalSource", externalSource);
            cmd.AddParameterWithValue("ExternalSource", externalSource);

            await using var reader = await cmd.ExecuteReaderAsyncEnsureRowAsync().ConfigureAwait(false);
            await reader.ReadAsync(token).ConfigureAwait(false);

            // Map, append and return product.
            var product = MapFromReader(reader);
            product.FullDescription = _descriptionService.GenerateFullDescription(product);
            product.TerrainDescription = _descriptionService.GenerateTerrainDescription(product);
            return product;
        }

        public async Task<AnalysisProduct> GetByIdAsync(Guid userId, string id, CancellationToken token)
        {
            // Validate parameters.
            userId.ThrowIfNullOrEmpty();
            id.ThrowIfNullOrEmpty();
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            //// Build sql.
            //var sql = @"
            //    SELECT 
            //        * 
            //    FROM geocoder.analysis_complete AS ac
            //    WHERE ac.id = @ExternalId";

            //// Execute sql.
            //await using var connection = await DbProvider.OpenConnectionScopeAsync().ConfigureAwait(false);
            //await using var cmd = DbProvider.CreateCommand(sql, connection);
            //cmd.AddParameterWithValue("ExternalId", externalId);
            //cmd.AddParameterWithValue("ExternalSource", externalSource);

            //await using var reader = await cmd.ExecuteReaderAsyncEnsureRowAsync().ConfigureAwait(false);
            //await reader.ReadAsync(token).ConfigureAwait(false);

            //// Map, append and return product.
            //var product = MapFromReader(reader);
            //product.FullDescription = _descriptionService.GenerateFullDescription(product);
            //product.TerrainDescription = _descriptionService.GenerateTerrainDescription(product);
            //return product;

            throw new NotImplementedException();
        }

        public Task<IEnumerable<AnalysisProduct>> GetByQueryAsync(Guid userId, string query, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// TODO How to handle doubles?
        /// TODO Make additional extensions?
        /// <summary>
        ///     Maps a reader to an <see cref="AnalysisProduct"/>.
        /// </summary>
        /// <param name="reader"><see cref="DbDataReader"/></param>
        /// <returns><see cref="AnalysisProduct"/></returns>
        private static AnalysisProduct MapFromReader(DbDataReader reader)
            => new AnalysisProduct
            {
                Id = reader.SafeGetString(0),
                ExternalId = reader.SafeGetString(1),
                ExternalSource = reader.GetFieldValue<ExternalDataSource>(2),
                FoundationType = reader.GetFieldValue<FoundationType>(3),
                GroundWaterLevel = reader.GetDouble(4),
                FoundationRisk = reader.GetFieldValue<FoundationRisk>(5),
                ConstructionYear = DateTimeOffsetHelper.FromYear(reader.GetSafeInt(6) ?? 0), // TODO Make extension, clean up
                BuildingHeight = reader.GetDouble(7),
                GroundLevel = reader.GetFloat(8),
                RestorationCosts = reader.GetDouble(9),
                DewateringDepth = reader.GetDouble(10),
                DryPeriod = reader.GetDouble(11),
                Reliability = reader.GetDouble(12),
            };
    }
}
#pragma warning restore CA1812 // Internal class is never instantiated
