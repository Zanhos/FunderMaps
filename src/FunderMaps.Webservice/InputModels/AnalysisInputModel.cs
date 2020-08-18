﻿using FunderMaps.Core.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FunderMaps.Webservice.InputModels
{
    /// <summary>
    ///     DTO for an analysis product request.
    ///     TODO This should check for invalid combinations.
    /// </summary>
    public sealed class AnalysisInputModel : PaginationInputModel, IValidatableObject
    {
        /// <summary>
        ///     Product type.
        /// </summary>
        [Required]
        public string Product { get; set; }

        /// <summary>
        ///     Query search string.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        ///     Internal id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     External BAG id.
        /// </summary>
        public string BagId { get; set; }

        /// <summary>
        ///     Bool indicating if we want everything in our fence.
        /// </summary>
        public bool FullFence { get; set; } = false;

        /// <summary>
        ///     Validate this object.
        /// </summary>
        /// <remarks>
        ///     This checks if only one of <see cref="Query"/>, <see cref="Id"/>, 
        ///     <see cref="BagId"/> and <see cref="FullFence"/> is set to true or
        ///     notnull.
        /// </remarks>
        /// <param name="validationContext"><see cref="ValidationContext"/></param>
        /// <returns><see cref="IEnumerable{ValidationResult}"/></returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FullFence && StringCollectionHelper.NotNullCount(Query, Id, BagId) != 0 ||
                !FullFence && StringCollectionHelper.NotNullCount(Query, Id, BagId) != 1)
            {
                yield return new ValidationResult("Please select one of the following: id, bagid, query or fullfence");
            }
        }
    }
}