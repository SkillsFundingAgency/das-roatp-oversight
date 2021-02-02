using System;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmOutcomeViewModel
    {
        public Guid ApplicationId { get; set; }
        public Guid OutcomeKey { get; set; }
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public string ApplicationStatus { get; set; }
        public string ApplicationEmailAddress { get; set; }

        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }
        public OversightReviewStatus OversightStatus { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
        
        public string Confirm { get; set; }

        public string PageTitle
        {
            get
            {
                var statusLabel = "";
                switch (OversightStatus)
                {
                    case OversightReviewStatus.Successful:
                    case OversightReviewStatus.SuccessfulAlreadyActive:
                    case OversightReviewStatus.SuccessfulFitnessForFunding:
                        statusLabel = "successful";
                        break;
                    case OversightReviewStatus.InProgress:
                        statusLabel = "'in progress'";
                        break;
                    case OversightReviewStatus.Unsuccessful:
                        statusLabel = "unsuccessful";
                        break;
                    default:
                        statusLabel = string.Empty;
                        break;
                }
                return $"Are you sure you want to mark this application as {statusLabel}?";
            }
        }

        public string OversightStatusLabel
        {
            get
            {
                switch(OversightStatus)
                {
                    case OversightReviewStatus.Successful:
                        return "Successful";
                    case OversightReviewStatus.SuccessfulAlreadyActive:
                        return "Successful - already active";
                    case OversightReviewStatus.SuccessfulFitnessForFunding:
                        return "Successful - fitness for funding";
                    case OversightReviewStatus.InProgress:
                        return "'in progress'";
                    case OversightReviewStatus.Unsuccessful:
                        return "unsuccessful";
                        break;
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
