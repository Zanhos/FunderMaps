﻿namespace FunderMaps.Webservice.ResponseModels.Types
{
    /// <summary>
    ///     Response model representing statistic product types.
    /// </summary>
    public enum StatisticsProductTypeResponseModel
    {
        /// <summary>
        /// Area analysis on the ratio of foundation types.
        /// </summary>
        FoundationRatio,

        /// <summary>
        /// Area analysis on the distribution of built years.
        /// </summary>
        ConstructionYears,

        /// <summary>
        /// Area analysis on building foundation risk.
        /// </summary>
        FoundationRisk,

        /// <summary>
        /// Area analysis on the percentage of data collected.
        /// </summary>
        DataCollected,

        /// <summary>
        /// Area analysis on the amount of restored buildings.
        /// </summary>
        BuildingsRestored,

        /// <summary>
        /// Area analysis on the amount of incidents.
        /// </summary>
        Incidents,

        /// <summary>
        /// Area analysis on the amount of reports.
        /// </summary>
        Reports,
    }
}