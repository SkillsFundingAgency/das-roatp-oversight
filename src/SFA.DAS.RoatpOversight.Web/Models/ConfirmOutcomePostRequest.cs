using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmOutcomePostRequest : OutcomePostRequest
    {
        public Guid OutcomeKey { get; set; }
        public string Confirm { get; set; }
    }
}
