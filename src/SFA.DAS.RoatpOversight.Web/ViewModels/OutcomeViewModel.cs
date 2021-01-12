using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;

namespace SFA.DAS.RoatpOversight.Web.ViewModels
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
     
        public List<ValidationErrorDetail> ErrorMessages { get; set; }

        public string ApplicationEmailAddress { get; set; }
        public string AssessorReviewStatus { get; set; }

        public string GatewayReviewStatus { get; set; }
        public DateTime? GatewayOutcomeMadeDate { get; set; }

        public string GatewayOutcomeMadeBy { get; set; }
        public string GatewayComments { get; set; }

        public string FinancialReviewStatus { get; set; }
        public string FinancialGradeAwarded { get; set; }

        public DateTime? FinancialHealthAssessedOn { get; set; }
        public string FinancialHealthAssessedBy { get; set; }
        public string FinancialHealthComments { get; set; }

        public string ModerationReviewStatus { get; set; }

        public DateTime? ModerationOutcomeMadeOn { get; set; }
        public string ModeratedBy { get; set; }
        public string ModerationComments { get; set; }


        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }
        public string OversightStatus { get; set; }

        public string SuccessfulText { get; set; }  

        public string SuccessfulAlreadyActiveText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }

        public string AssessmentOutcome
        {
            get
            {
           
                var financialDetailsPass = false;
                if (FinancialReviewStatus == Domain.FinancialReviewStatus.Exempt)
                    financialDetailsPass = true;
                else
                {
                    if (FinancialReviewStatus ==  Domain.FinancialReviewStatus.Pass && 
                            (FinancialGradeAwarded == FinancialApplicationSelectedGrade.Exempt ||
                            FinancialGradeAwarded == FinancialApplicationSelectedGrade.Outstanding ||
                            FinancialGradeAwarded == FinancialApplicationSelectedGrade.Good ||
                            FinancialGradeAwarded == FinancialApplicationSelectedGrade.Satisfactory))
                        financialDetailsPass = true;
                }


                if (GatewayReviewStatus == Domain.GatewayReviewStatus.Pass && ModerationReviewStatus == Domain.ModerationReviewStatus.Pass &&
                    financialDetailsPass)
                    return AssessmentOutcomeStatus.Passed;

                return AssessmentOutcomeStatus.Failed;
            }
        }
    }
}
