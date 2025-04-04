using System.Collections.Generic;
using Sem3EProjectOnlineCPFH.Models.Data;

namespace Sem3EProjectOnlineCPFH.Models.ViewModels
{
    public class VacancyViewModel
    {
        public Vacancy Vacancy { get; set; }
        public List<InterviewViewModel> Interviews { get; set; } = new List<InterviewViewModel>();
        public List<Applicant> Applicants { get; set; }
    }
}
