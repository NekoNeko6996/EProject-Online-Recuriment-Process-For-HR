using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sem3EProjectOnlineCPFH.Models.Data
{
    public class Applicant_Vacancy
    {
        [Key]
        public int ApplicantVacancyId { get; set; }

        public int ApplicantId { get; set; }
        public int VacancyId { get; set; }
        public int Approver { get; set; }
        public DateTime ApplyAt { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicant Applicant { get; set; }

        [ForeignKey("VacancyId")]
        public virtual Vacancy Vacancy { get; set; }
    }
}