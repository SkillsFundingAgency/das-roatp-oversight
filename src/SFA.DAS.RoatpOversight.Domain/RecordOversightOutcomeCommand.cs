using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class RecordOversightOutcomeCommand 
    {
        public Guid ApplicationId { get; set; }
        public string OversightStatus { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
