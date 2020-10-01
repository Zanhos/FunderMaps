﻿using System;

namespace FunderMaps.WebApi.DataTransferObjects
{
    /// <summary>
    ///     Attribution data transfer object.
    /// </summary>
    public sealed class AttributionDto
    {
        /// <summary>
        ///     Unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Reviewer idenitfier.
        /// </summary>
        public Guid? Reviewer { get; set; }

        /// <summary>
        ///     Creator identifier.
        /// </summary>
        public Guid Creator { get; set; }

        /// <summary>
        ///     Owner identifier.
        /// </summary>
        public Guid Owner { get; set; }

        /// <summary>
        ///     Contractor identifier.
        /// </summary>
        public Guid Contractor { get; set; }
    }
}