﻿using System.Collections.Generic;
using System.Linq;

namespace FunderMaps.Core.Types.Distributions
{
    /// <summary>
    /// Represents a distribution of construction years.
    /// </summary>
    public sealed class ConstructionYearDistribution
    {
        /// <summary>
        /// Represents each decade in which the construction year of one or 
        /// more buildings exist, including the total amount of buildings per
        /// decade.
        /// </summary>
        public IEnumerable<ConstructionYearPair> Decades { get; set; }
    }
}