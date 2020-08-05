﻿using System;

namespace FunderMaps.Core.Extensions
{
    /// <summary>
    /// Contains extension functionality for the <see cref="Guid"/> type.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the <paramref name="self"/>
        /// Guid is either null or empty.
        /// </summary>
        /// <param name="self"><see cref="Guid"/></param>
        public static void ThrowIfNullOrEmpty(this Guid self)
        {
            if (self == null || self == Guid.Empty) { throw new ArgumentNullException(nameof(self)); }
        }
    }
}