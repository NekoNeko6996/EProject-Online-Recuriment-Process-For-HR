using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Data
{
    public class Applicant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ApplicantId { get; set; }

        [Required, MaxLength(50)]
        public string FullName { get; set; }

        [Required, MaxLength(256)]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string CVPath { get; set; }
        public string AvatarPath { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AttachedVacancies { get; set; }

        public virtual ICollection<Applicant_Vacancy> ApplicantVacancies { get; set; }
    }
}