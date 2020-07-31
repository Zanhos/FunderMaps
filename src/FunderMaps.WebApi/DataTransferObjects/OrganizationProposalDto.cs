﻿using System;
using System.ComponentModel.DataAnnotations;

namespace FunderMaps.WebApi.DataTransferObjects
{
    /// <summary>
    ///     Organization proposal DTO.
    /// </summary>
    public class OrganizationProposalDto
    {
        /// <summary>
        ///     Organization identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Organization name.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        /// <summary>
        ///     Organization email address.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}