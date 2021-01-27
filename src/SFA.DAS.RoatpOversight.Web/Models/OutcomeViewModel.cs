using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class OutcomeViewModel
    {
        public ApplicationSummaryViewModel ApplicationSummary { get; set; }

        public GatewayOutcomeViewModel GatewayOutcome { get; set; }

        public FinancialHealthOutcomeViewModel FinancialHealthOutcome { get; set; }
        public ModerationOutcomeViewModel ModerationOutcome { get; set; }

        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }
        public OversightReviewStatus OversightStatus { get; set; }

        public string SuccessfulText { get; set; }  

        public string SuccessfulAlreadyActiveText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string UnsuccessfulExternalText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }
        public string ApplicantEmailAddress { get; set; }
    }
}
