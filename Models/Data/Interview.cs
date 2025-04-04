using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Sem3EProjectOnlineCPFH.Models.Auth;
using Sem3EProjectOnlineCPFH.Models.Enum;

namespace Sem3EProjectOnlineCPFH.Models.Data
{
    public class Interview : IValidatableObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string InterviewId { get; set; }

        [Required]
        public string ApplicantId { get; set; }

        [Required]
        public string VacancyId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public string InterviewerId { get; set; }

        [Required]
        public string InterviewMethod { get; set; }

        public string InterviewURL { get; set; } // Không đặt [Required] để kiểm tra bằng logic

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public InterviewStatus Status { get; set; }
        public string Result { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicant Applicant { get; set; }

        [ForeignKey("InterviewerId")]
        public virtual ApplicationUser Interviewer { get; set; }

        [ForeignKey("VacancyId")]
        public virtual Vacancy Vacancy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (InterviewMethod == "Online" && string.IsNullOrWhiteSpace(InterviewURL))
            {
                errors.Add(new ValidationResult("Interview URL is required for Online interviews.", new[] { "InterviewURL" }));
            }

            if (EndTime <= StartTime)
            {
                errors.Add(new ValidationResult("End time must be greater than start time.", new[] { "EndTime" }));
            }

            return errors;
        }
    }
}
