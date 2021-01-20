using System;
using System.Linq;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class OverallOutcomeDetails
    {
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public DateTime? ApplicationDeterminedDate { get; set; }
        public string OversightStatus { get; set; }
        public string ApplicationStatus { get; set; }

        public string Status =>
            OversightReviewStatus.SuccessfulStatuses.Contains(OversightStatus)
                ? OversightReviewStatus.Successful
                : OversightStatus;
    }
}
