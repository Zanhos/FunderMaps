﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace FunderMaps.Core.Entities.Fis
{
    public class Principal : BaseEntity
    {
        public Principal()
        {
            AttributionReviewerNavigation = new HashSet<Attribution>();
            AttributionCreatorNavigation = new HashSet<Attribution>();
            Incident = new HashSet<Incident>();
            ProjectAdviserNavigation = new HashSet<Project>();
            ProjectCreatorNavigation = new HashSet<Project>();
            ProjectLeadNavigation = new HashSet<Project>();
        }

        public int Id { get; set; }

        [MaxLength(256)]
        public string NickName { get; set; }

        [MaxLength(256)]
        public string FirstName { get; set; }
        [MaxLength(256)]
        public string MiddleName { get; set; }
        [MaxLength(256)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [MaxLength(256)]
        public string Email { get; set; }

        [IgnoreDataMember]
        public int? _Organization { get; set; }

        [MaxLength(16)]
        public string Phone { get; set; }

        public virtual Organization Organization { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Attribution> AttributionReviewerNavigation { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Attribution> AttributionCreatorNavigation { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Incident> Incident { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Project> ProjectAdviserNavigation { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Project> ProjectCreatorNavigation { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<Project> ProjectLeadNavigation { get; set; }
    }
}