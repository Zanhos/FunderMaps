using FunderMaps.Core.Types;
using System;
using System.Collections.Generic;

namespace FunderMaps.BatchNode.Jobs.BundleBuilder
{
    /// <summary>
    ///     Context for executing a bundle build task.
    /// </summary>
    public class BundleBuildingContext // TODO: Remove, this is moved to core
    {
        /// <summary>
        ///     The id of the bundle to process.
        /// </summary>
        public Guid BundleId { get; set; }

        /// <summary>
        ///     Contains all formats we wish to export.
        /// </summary>
        public IEnumerable<GeometryFormat> Formats { get; set; }
    }
}