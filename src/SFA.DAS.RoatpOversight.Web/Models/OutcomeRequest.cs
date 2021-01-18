using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class OutcomeRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid? OutcomeKey { get; set; }
    }
}
