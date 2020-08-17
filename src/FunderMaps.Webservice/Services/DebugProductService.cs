﻿using FunderMaps.Core.Helpers;
using FunderMaps.Core.Interfaces;
using FunderMaps.Core.Types;
using FunderMaps.Core.Types.Distributions;
using FunderMaps.Core.Types.Products;
using FunderMaps.Webservice.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FunderMaps.Webservice.Services
{    
    /// TODO Make Faker.
     /// <summary>
     ///     Used for testing purposes.
     /// </summary>
    public sealed class DebugProductService : IProductService
    {
        public Task<AnalysisProduct> GetAnalysisByExternalIdAsync(Guid userId, AnalysisProductType productType, string externalId, ExternalDataSource externalSource, INavigation navigation, CancellationToken token)
            => Task.FromResult(GetAnalysisDummy());

        public Task<AnalysisProduct> GetAnalysisByIdAsync(Guid userId, AnalysisProductType productType, string id, INavigation navigation, CancellationToken token)
            => Task.FromResult(GetAnalysisDummy());

        public async Task<IEnumerable<AnalysisProduct>> GetAnalysisByQueryAsync(Guid userId, AnalysisProductType productType, string query, INavigation navigation, CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return new List<AnalysisProduct>
            {
                GetAnalysisDummy(),
                GetAnalysisDummy(),
                GetAnalysisDummy()
            };
        }

        public async Task<IEnumerable<AnalysisProduct>> GetAnalysisInFenceAsync(Guid userId, AnalysisProductType productType, INavigation navigation, CancellationToken token)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return new List<AnalysisProduct>
            {
                GetAnalysisDummy(),
                GetAnalysisDummy(),
                GetAnalysisDummy()
            };
        }

        public Task<StatisticsProduct> GetStatisticsByAreaAsync(Guid userId, StatisticsProductType productType, string areaId, INavigation navigation, CancellationToken token) => throw new NotImplementedException();


        public Task<StatisticsProduct> GetStatisticsInFenceAsync(Guid userId, StatisticsProductType productType, INavigation navigation, CancellationToken token) => throw new NotImplementedException();

        /// <summary>
        /// Creates a new <see cref="AnalysisProduct"/> dummy where all fields
        /// have been populated by some random data.
        /// </summary>
        /// <returns><see cref="AnalysisProduct"/></returns>
        private static AnalysisProduct GetAnalysisDummy() => new AnalysisProduct
        {
            BuildingHeight = 100,
            ConstructionYear = DateTimeOffsetHelper.FromYear(1995),
            ConstructionYearDistribution = new ConstructionYearDistribution
            {
                Decades = new List<ConstructionYearPair> {
                        new ConstructionYearPair { Decade = Years.FromDecade(1950), TotalCount = 18 },
                        new ConstructionYearPair { Decade = Years.FromDecade(1960), TotalCount = 22 },
                        new ConstructionYearPair { Decade = Years.FromDecade(1970), TotalCount = 20 },
                    }
            },
            DataCollectedPercentage = 54,
            DewateringDepth = 12,
            DryPeriod = 25,
            ExternalId = "NL.IMBAG.PAND.285747389504823",
            ExternalSource = ExternalDataSource.NlBag,
            FoundationRisk = FoundationRisk.A,
            FoundationRiskDistribution = new FoundationRiskDistribution
            {
                PercentageA = 15,
                PercentageB = 25,
                PercentageC = 10,
                PercentageD = 10,
                PercentageE = 40
            },
            FoundationType = FoundationType.Concrete,
            FoundationTypeDistribution = new FoundationTypeDistribution
            {
                FoundationTypes = new List<FoundationTypePair>
                    {
                        new FoundationTypePair {FoundationType = FoundationType.Concrete, TotalCount = 26},
                        new FoundationTypePair {FoundationType = FoundationType.NoPileSlit, TotalCount = 12},
                        new FoundationTypePair {FoundationType = FoundationType.Other, TotalCount = 22}
                    }
            },
            FullDescription = "This is my full building description",
            GroundLevel = 15,
            GroundWaterLevel = 12,
            Id = $"gfm-{Guid.NewGuid()}",
            Reliability = Reliability.Indicative,
            RestorationCosts = 1434,
            TerrainDescription = "This is my terrain description",
            TotalBuildingRestoredCount = 23,
            TotalIncidentCount = 15,
            TotalReportCount = 13
        };
    }
}
