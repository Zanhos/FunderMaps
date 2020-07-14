﻿using FunderMaps.Core.DataAnnotations;
using FunderMaps.Core.Types;
using System.ComponentModel.DataAnnotations;

namespace FunderMaps.Core.Entities
{
    // TODO: Move validation on properties.
    /// <summary>
    ///     Indicent entity.
    /// </summary>
    public class Incident : RecordControl
    {
        /// <summary>
        /// Unique identifier.
        /// </summary>
        [Incident]
        public string Id { get; set; }

        /// <summary>
        /// Client identifier.
        /// </summary>
        [Required, Range(1, 99)]
        public int ClientId { get; set; }

        /// <summary>
        /// Foundation type.
        /// </summary>
        public FoundationType FoundationType { get; set; }

        /// <summary>
        /// Building chained to another building.
        /// </summary>
        public bool ChainedBuilding { get; set; }

        /// <summary>
        /// Whether the contact is an owner of the building.
        /// </summary>
        public bool Owner { get; set; }

        /// <summary>
        /// Whether foundation was recovered or not.
        /// </summary>
        public bool FoundationRecovery { get; set; }

        /// <summary>
        /// Whether neighbor foundation was recovered or not.
        /// </summary>
        public bool NeightborRecovery { get; set; }

        /// <summary>
        /// Foundation damage cause.
        /// </summary>
        public FoundationDamageCause FoundationDamageCause { get; set; }

        /// <summary>
        /// Document name.
        /// </summary>
        //[Url]
        public string[] DocumentFile { get; set; }

        /// <summary>
        ///     Note.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Internal note.
        /// </summary>
        public string InternalNote { get; set; }

        /// <summary>
        /// Fouindational damage.
        /// </summary>
        public FoundationDamageCharacteristics[] FoundationDamageCharacteristics { get; set; }

        /// <summary>
        /// Environmental damage.
        /// </summary>
        public EnvironmentDamageCharacteristics[] EnvironmentDamageCharacteristics { get; set; }

        /// <summary>
        /// Contact email.
        /// </summary>
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Address identifier.
        /// </summary>
        [Required, Address]
        public string Address { get; set; }

        /// <summary>
        /// Audit status.
        /// </summary>
        public AuditStatus AuditStatus { get; set; }

        /// <summary>
        /// Question type.
        /// </summary>
        public IncidentQuestionType QuestionType { get; set; }

        /// <summary>
        /// Meta data.
        /// </summary>
        public object Meta { get; set; }

        /// <summary>
        /// Contact object.
        /// </summary>
        public Contact ContactNavigation { get; set; }

        /// <summary>
        /// Address object.
        /// </summary>
        public Address AddressNavigation { get; set; }
    }
}
