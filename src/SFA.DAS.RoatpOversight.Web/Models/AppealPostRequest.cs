using System;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealPostRequest
    {
        public Guid ApplicationId { get; set; }

        public string AppealStatus { get; set; }
        //   public string ApproveGateway { get; set; }
        //   public string ApproveModeration { get; set; }
        public string SuccessfulText { get; set; }
        // public string SuccessfulExternalText { get; set; }
        public string SuccessfulAlreadyActiveText { get; set; }
        //    public string SuccessfulAlreadyActiveExternalText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string UnsuccessfulExternalText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }
        //   public bool IsGatewayFail { get; set; }
        //   public bool IsGatewayRemoved { get; set; }
        //    public string UnsuccessfulPartiallyUpheldText { get; set; }
        //    public string UnsuccessfulPartiallyUpheldExternalText { get; set; }

    }
}