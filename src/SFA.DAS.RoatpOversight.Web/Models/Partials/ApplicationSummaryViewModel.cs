using System;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class ApplicationSummaryViewModel
    {
        public Guid ApplicationId { get; set; }
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public string ApplicationStatus { get; set; }

        public string ApplicationEmailAddress { get; set; }
        public string AssessorReviewStatus { get; set; }
        public string AssessmentOutcome { get; set; }
        public bool ShowAssessmentOutcome { get; set; }
    }
}
