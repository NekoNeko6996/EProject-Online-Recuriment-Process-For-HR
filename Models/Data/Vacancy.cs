using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Sem3EProjectOnlineCPFH.Models.Auth;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Data
{
    public class Vacancy
    {
        [Key]
        public int VacancyId { get; set; }

        public string OwnerId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }
        public int DepartmentId { get; set; }
        public string Status { get; set; }
        public int NumberOfOpenings { get; set; }
        public DateTime Deadline { get; set; }
        public int ApplicationLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ListOfSelectedApplicants { get; set; }

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }


        //
        public virtual ICollection<Applicant_Vacancy> ApplicantVacancies { get; set; } = new List<Applicant_Vacancy>();
    }
}