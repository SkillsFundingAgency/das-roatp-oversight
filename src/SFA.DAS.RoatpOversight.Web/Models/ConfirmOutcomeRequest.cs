using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmOutcomeRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid OutcomeKey { get; set; }
    }
}
