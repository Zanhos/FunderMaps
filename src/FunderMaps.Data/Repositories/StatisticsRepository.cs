﻿using FunderMaps.Core.Interfaces.Repositories;
using FunderMaps.Core.Types;
using FunderMaps.Core.Types.Distributions;
using FunderMaps.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1812 // Internal class is never instantiated
namespace FunderMaps.Data.Repositories
{
    /// <summary>
    ///     Repository for statistics.
    /// </summary>
    internal sealed class StatisticsRepository : DbContextBase, IStatisticsRepository
    {
        /// <summary>
        ///     Get foundation type distribution by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<FoundationTypeDistribution> GetFoundationTypeDistributionByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- FoundationTypeDistribution
                        spft.foundation_type,
                        round(spft.percentage::numeric, 2)
                FROM    data.statistics_product_foundation_type AS spft
                JOIN    geocoder.neighborhood n ON n.id = spft.neighborhood_id 
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            List<FoundationTypePair> pairs = new();
            await foreach (var reader in context.EnumerableReaderAsync())
            {
                pairs.Add(new()
                {
                    FoundationType = reader.GetFieldValue<FoundationType>(0),
                    Percentage = reader.GetDecimal(1)
                });
            }

            return new()
            {
                FoundationTypes = pairs
            };
        }

        /// <summary>
        ///     Get foundation type distribution by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<FoundationTypeDistribution> GetFoundationTypeDistributionByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- FoundationTypeDistribution
                        spft.foundation_type,
                        round(spft.percentage::numeric, 2)
                FROM    data.statistics_product_foundation_type AS spft
                JOIN    geocoder.neighborhood n ON n.id = spft.neighborhood_id 
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            List<FoundationTypePair> pairs = new();
            await foreach (var reader in context.EnumerableReaderAsync())
            {
                pairs.Add(new()
                {
                    FoundationType = reader.GetFieldValue<FoundationType>(0),
                    Percentage = reader.GetDecimal(1)
                });
            }

            return new()
            {
                FoundationTypes = pairs
            };
        }

        /// <summary>
        ///     Get construction year distribution by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<ConstructionYearDistribution> GetConstructionYearDistributionByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- ConstructionYearDistribution
                        spcy.year_from,
                        spcy.count
                FROM    data.statistics_product_construction_years AS spcy
                JOIN    geocoder.neighborhood n ON n.id = spcy.neighborhood_id 
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            List<ConstructionYearPair> pairs = new();
            await foreach (var reader in context.EnumerableReaderAsync())
            {
                pairs.Add(new()
                {
                    Decade = Years.FromDecade(reader.GetInt(0)),
                    TotalCount = reader.GetInt(1)
                });
            }

            return new()
            {
                Decades = pairs
            };
        }

        /// <summary>
        ///     Get construction year distribution by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<ConstructionYearDistribution> GetConstructionYearDistributionByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- ConstructionYearDistribution
                        spcy.year_from,
                        spcy.count
                FROM    data.statistics_product_construction_years AS spcy
                JOIN    geocoder.neighborhood n ON n.id = spcy.neighborhood_id 
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            List<ConstructionYearPair> pairs = new();
            await foreach (var reader in context.EnumerableReaderAsync())
            {
                pairs.Add(new()
                {
                    Decade = Years.FromDecade(reader.GetInt(0)),
                    TotalCount = reader.GetInt(1)
                });
            }

            return new()
            {
                Decades = pairs
            };
        }

        /// <summary>
        ///     Get data collection percentage by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<decimal> GetDataCollectedPercentageByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- DataCollected
                        round(spdc.percentage::numeric, 2)
                FROM    data.statistics_product_data_collected AS spdc
                JOIN    geocoder.neighborhood n ON n.id = spdc.neighborhood_id
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<decimal>();
        }

        /// <summary>
        ///     Get data collection percentage by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<decimal> GetDataCollectedPercentageByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- DataCollected
                        round(spdc.percentage::numeric, 2)
                FROM    data.statistics_product_data_collected AS spdc
                JOIN    geocoder.neighborhood n ON n.id = spdc.neighborhood_id
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<decimal>();
        }

        /// <summary>
        ///     Get foundation risk distribution by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<FoundationRiskDistribution> GetFoundationRiskDistributionByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- FoundationRiskDistribution
                        spfr.foundation_risk,
                        round(spfr.percentage::numeric, 2)
                FROM    data.statistics_product_foundation_risk AS spfr
                JOIN    geocoder.neighborhood n ON n.id = spfr.neighborhood_id 
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            Dictionary<FoundationRisk, decimal> map = new()
            {
                { FoundationRisk.A, 0 },
                { FoundationRisk.B, 0 },
                { FoundationRisk.C, 0 },
                { FoundationRisk.D, 0 },
                { FoundationRisk.E, 0 }
            };

            await foreach (var reader in context.EnumerableReaderAsync())
            {
                map[reader.GetFieldValue<FoundationRisk>(0)] = reader.GetDecimal(1);
            }

            return new()
            {
                PercentageA = map[FoundationRisk.A],
                PercentageB = map[FoundationRisk.B],
                PercentageC = map[FoundationRisk.C],
                PercentageD = map[FoundationRisk.D],
                PercentageE = map[FoundationRisk.E]
            };
        }

        /// <summary>
        ///     Get foundation risk distribution by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<FoundationRiskDistribution> GetFoundationRiskDistributionByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- FoundationRiskDistribution
                        spfr.foundation_risk,
                        round(spfr.percentage::numeric, 2)
                FROM    data.statistics_product_foundation_risk AS spfr
                JOIN    geocoder.neighborhood n ON n.id = spfr.neighborhood_id 
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            Dictionary<FoundationRisk, decimal> map = new()
            {
                { FoundationRisk.A, 0 },
                { FoundationRisk.B, 0 },
                { FoundationRisk.C, 0 },
                { FoundationRisk.D, 0 },
                { FoundationRisk.E, 0 }
            };

            await foreach (var reader in context.EnumerableReaderAsync())
            {
                map[reader.GetFieldValue<FoundationRisk>(0)] = reader.GetDecimal(1);
            }

            return new()
            {
                PercentageA = map[FoundationRisk.A],
                PercentageB = map[FoundationRisk.B],
                PercentageC = map[FoundationRisk.C],
                PercentageD = map[FoundationRisk.D],
                PercentageE = map[FoundationRisk.E]
            };
        }

        /// <summary>
        ///     Get total building restored count by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<long> GetTotalBuildingRestoredCountByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- BuildingRestoredCount
                        spbr.count
                FROM    data.statistics_product_buildings_restored AS spbr
                JOIN    geocoder.neighborhood n ON n.id = spbr.neighborhood_id
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<long>();
        }

        /// <summary>
        ///     Get total building restored count by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<long> GetTotalBuildingRestoredCountByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- BuildingRestoredCount
                        spbr.count
                FROM    data.statistics_product_buildings_restored AS spbr
                JOIN    geocoder.neighborhood n ON n.id = spbr.neighborhood_id
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<long>();
        }

        /// <summary>
        ///     Get total incident count by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<long> GetTotalIncidentCountByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- IncidentCount
                        spi.count
                FROM    data.statistics_product_incidents AS spi
                JOIN    geocoder.neighborhood n ON n.id = spi.neighborhood_id
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<long>();
        }

        /// <summary>
        ///     Get total incident count by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<long> GetTotalIncidentCountByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- IncidentCount
                        spi.count
                FROM    data.statistics_product_incidents AS spi
                JOIN    geocoder.neighborhood n ON n.id = spi.neighborhood_id
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<long>();
        }

        /// <summary>
        ///     Get total report count by id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<long> GetTotalReportCountByIdAsync(string id)
        {
            var sql = @"
                SELECT  -- ReportCount
                        spi2.count
                FROM    data.statistics_product_inquiries AS spi2
                JOIN    geocoder.neighborhood n ON n.id = spi2.neighborhood_id
                WHERE   n.id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<long>();
        }

        /// <summary>
        ///     Get total report count by external id.
        /// </summary>
        /// <param name="id">Neighborhood identifier.</param>
        public async Task<long> GetTotalReportCountByExternalIdAsync(string id)
        {
            var sql = @"
                SELECT  -- ReportCount
                        spi2.count
                FROM    data.statistics_product_inquiries AS spi2
                JOIN    geocoder.neighborhood n ON n.id = spi2.neighborhood_id
                WHERE   n.external_id = @id";

            // FUTURE: Maybe move up.
            if (AppContext.HasIdentity)
            {
                sql += $"\r\n AND application.is_geometry_in_fence(@user_id, n.geom)";
            }

            sql += $"\r\n LIMIT 1";

            await using var context = await DbContextFactory(sql);

            context.AddParameterWithValue("id", id);

            if (AppContext.HasIdentity)
            {
                context.AddParameterWithValue("user_id", AppContext.UserId);
            }

            return await context.ScalarAsync<long>();
        }
    }
}
#pragma warning disable CA1812 // Internal class is never instantiated
