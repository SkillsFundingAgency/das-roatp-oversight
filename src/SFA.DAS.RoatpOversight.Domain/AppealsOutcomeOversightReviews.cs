using SFA.DAS.ApplyService.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class AppealsOutcomeOversightReviews
    {
        public List<AppealsOutcomeOversightReview> Reviews { get; set; }
    }

    public class AppealsOutcomeOversightReview
    {
        public Guid ApplicationId { get; set; }
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime? AppealSubmittedDate { get; set; }
        public DateTime? AppealDeterminedDate { get; set; }
        public string ApplicationStatus { get; set; }
        public string AppealOutcome { get; set; }

    }
}
