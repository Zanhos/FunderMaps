﻿using FunderMaps.Core.DataAnnotations;
using FunderMaps.Core.Entities.Geocoder;
using System.ComponentModel.DataAnnotations;

namespace FunderMaps.Core.Entities
{
    /// <summary>
    ///     Access entity.
    /// </summary>
    public sealed class Address : IdentifiableEntity<Address, string>, IGeocoderEntity<Address>
    {
        /// <summary>
        ///     Create new instance.
        /// </summary>
        public Address()
            : base(e => e.Id)
        {
        }

        /// <summary>
        ///     Unique identifier.
        /// </summary>
        [Required, Address]
        public string Id { get; set; }

        /// <summary>
        ///     Building number.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string BuildingNumber { get; set; }

        /// <summary>
        ///     Postcode.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        ///     Street name.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Street { get; set; }

        /// <summary>
        ///     Address is active or not.
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        ///     External data source id.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string ExternalId { get; set; }

        // TODO: This is a type, see #211
        /// <summary>
        ///     External data source.
        /// </summary>
        [Required]
        public string ExternalSource { get; set; }

        /// <summary>
        ///     Print object as name.
        /// </summary>
        /// <returns>String representing user.</returns>
        public override string ToString() => Id;
    }
}
