using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmAppealOutcomePostRequest
    {
        public Guid ApplicationId { get; set; }
        public string AppealStatus { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
        public Guid OutcomeKey { get; set; }
        public string Confirm { get; set; }
    }
}