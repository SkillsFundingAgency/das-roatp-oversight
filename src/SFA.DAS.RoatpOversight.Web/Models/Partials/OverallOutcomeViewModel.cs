using System;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class OverallOutcomeViewModel
    {
        public OversightReviewStatus OversightStatus { get; set; }
        public DateTime? ApplicationDeterminedDate { get; set; }
        public string OversightUserName { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
        public bool IsGatewayOutcome { get; set; }
        public bool ShowHeading => !IsGatewayOutcome;
        public string DateLabel => IsGatewayOutcome ? "Outcome made date" : "Application determined date";

        public bool ShowSpecificStatus => OversightStatus == OversightReviewStatus.SuccessfulAlreadyActive ||
                                          OversightStatus == OversightReviewStatus.SuccessfulFitnessForFunding;
    }
}
