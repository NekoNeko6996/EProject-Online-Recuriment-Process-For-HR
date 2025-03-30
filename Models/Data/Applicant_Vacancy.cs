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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ApplicantVacancyId { get; set; }

        public string ApplicantId { get; set; }
        public string VacancyId { get; set; }
        public int Approver { get; set; }
        public DateTime ApplyAt { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicant Applicant { get; set; }

        [ForeignKey("VacancyId")]
        public virtual Vacancy Vacancy { get; set; }
    }
}