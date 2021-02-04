using System;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class OutcomePostRequest
    {
        public Guid ApplicationId { get; set; }

        public OversightReviewStatus OversightStatus { get; set; }
        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }
        public string SuccessfulText { get; set; }
        public string SuccessfulAlreadyActiveText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string UnsuccessfulExternalText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }
        public bool IsGatewayFail { get; set; }
        public bool IsGatewayRemoved { get; set; }
    }
}
