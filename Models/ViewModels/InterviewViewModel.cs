using System;

namespace Sem3EProjectOnlineCPFH.Models.ViewModels
{
    public class InterviewViewModel
    {
        public string Id { get; set; }
        public string ApplicantId { get; set; }
        public string ApplicantName { get; set; }
        public string InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string InterviewMethod { get; set; }
        public string MeetUrl { get; set; }

        public string Status { get; set; }
    }
}
