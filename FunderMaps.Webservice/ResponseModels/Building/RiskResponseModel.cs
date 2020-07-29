﻿namespace FunderMaps.Webservice.ResponseModels.Building
{
    /// <summary>
    /// Represents a response model for the risk endpoint.
    /// </summary>
    public sealed class RiskResponseModel : BuildingResponseModelBase
    {
        /// <summary>
        /// Represents the foundation type of this building.
        /// </summary>
        public string FoundationType { get; set; }

        /// <summary>
        /// Represents the foundation risk for this building.
        /// </summary>
        public double FoundationRisk { get; set; }

        /// <summary>
        /// Description of the terrain on which this building lies.
        /// TODO Correct name?
        /// </summary>
        public string TerrainDescription { get; set; }

        /// <summary>
        /// Represents the estimated restoration costs for this building.
        /// TODO Correct unit?
        /// </summary>
        public double RestorationCosts { get; set; }

        /// <summary>
        /// Represents the dewatering depth (ontwateringsdiepte) for this building.
        /// TODO Correct unit?
        /// TODO Correct name?
        /// </summary>
        public double DewateringDepth { get; set; }

        /// <summary>
        /// Represents the period of drought (droogstand) for this building.
        /// TODO Correct unit?
        /// TODO Correct name?
        /// </summary>
        public double DryPeriod { get; set; }

        /// <summary>
        /// Represents the reliability of all data about this building.
        /// TODO What unit? Percentage?
        /// TODO Correct name?
        /// </summary>
        public double Reliability { get; set; }
    }
}
