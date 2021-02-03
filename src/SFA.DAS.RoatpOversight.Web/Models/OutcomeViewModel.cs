using System;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class OutcomeViewModel
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

        public string AssessmentOutcome
        {
            get
            {
                var financialDetailsPass = false;
                if (FinancialHealthOutcome.FinancialReviewStatus == FinancialReviewStatus.Exempt)
                    financialDetailsPass = true;
                else
                {
                    if (FinancialHealthOutcome.FinancialReviewStatus ==  FinancialReviewStatus.Pass && 
                            (FinancialHealthOutcome.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Exempt ||
                             FinancialHealthOutcome.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Outstanding ||
                             FinancialHealthOutcome.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Good ||
                             FinancialHealthOutcome.FinancialGradeAwarded == FinancialApplicationSelectedGrade.Satisfactory))
                        financialDetailsPass = true;
                }

                if (GatewayOutcome.GatewayReviewStatus == GatewayReviewStatus.Pass && ModerationOutcome.ModerationReviewStatus == ModerationReviewStatus.Pass &&
                    financialDetailsPass)
                    return AssessmentOutcomeStatus.Passed;

                return AssessmentOutcomeStatus.Failed;
            }
        }
    }
}
