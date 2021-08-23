using SFA.DAS.ApplyService.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class AppealsOversightReviews
    {
        public List<AppealsOversightReview> Reviews { get; set; }
    }

    public class AppealsOversightReview
    {
        public Guid ApplicationId { get; set; }
        public string OrganisationName { get; set; }

        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public DateTime? ApplicationDeterminedDate { get; set; }
        public DateTime? AppealSubmittedDate { get; set; }
        public OversightReviewStatus OversightStatus { get; set; }
        public string ApplicationStatus { get; set; }

    }
}
