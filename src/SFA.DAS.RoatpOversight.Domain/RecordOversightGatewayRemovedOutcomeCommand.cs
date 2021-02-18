using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class RecordOversightGatewayRemovedOutcomeCommand
    {
        public Guid ApplicationId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
