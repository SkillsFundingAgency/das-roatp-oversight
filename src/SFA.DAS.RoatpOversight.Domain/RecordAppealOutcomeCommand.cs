using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class RecordAppealOutcomeCommand
    {
        public Guid ApplicationId { get; set; }
        public string AppealStatus { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
    }
}