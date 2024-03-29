﻿using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class OutcomeViewModel
    {
        public ApplicationSummaryViewModel ApplicationSummary { get; set; }
        public bool IsNew { get; set; }
        public GatewayOutcomeViewModel GatewayOutcome { get; set; }

        public FinancialHealthOutcomeViewModel FinancialHealthOutcome { get; set; }
        public ModerationOutcomeViewModel ModerationOutcome { get; set; }
        public InProgressDetailsViewModel InProgressDetails { get; set; }
        public OverallOutcomeViewModel OverallOutcome { get; set; }
        public bool IsGatewayFail { get; set; }
        public bool IsGatewayRemoved { get; set; }
        public bool IsGatewayActionPendingConfirmation => IsGatewayFail || IsGatewayRemoved;
        public bool HasFinalOutcome { get; set; }
        public bool ShowInProgressDetails { get; set; }
     


        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }

        public string SuccessfulText { get; set; }
        public string SuccessfulExternalText { get; set; }

        public string SuccessfulAlreadyActiveText { get; set; }
        public string SuccessfulAlreadyActiveExternalText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string UnsuccessfulExternalText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }
        public OversightReviewStatus OversightStatus { get; set; }

        public string UnsuccessfulPartiallyUpheldText { get; set; }
        public string UnsuccessfulPartiallyUpheldExternalText { get; set; }

        public bool OnRegister { get; set; }
    }
}