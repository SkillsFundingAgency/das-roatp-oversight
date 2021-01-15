using System;
using System.Collections.Generic;
using System.ComponentModel;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.ViewModels
{
    public class OutcomeStatusViewModel
    {
        public Guid ApplicationId { get; set; }
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public string AreYouSureStatus { get; set; }

        public List<ValidationErrorDetail> ErrorMessages { get; set; }
        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }
        public string OversightStatus { get; set; }

        public string SuccessfulText { get; set; }
        public string SuccessfulAlreadyActiveText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }
        public string ApplicationEmailAddress { get; set; }
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
    }
}