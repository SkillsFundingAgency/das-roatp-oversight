using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class RecordOversightOutcomeCommand 
    {
        public Guid ApplicationId { get; set; }
        public string OversightStatus { get; set; }
        public DateTime ApplicationDeterminedDate { get; set; }
        public string UserName { get; set; }

        public RecordOversightOutcomeCommand()
        {

        }

        public RecordOversightOutcomeCommand(Guid applicationId, string oversightStatus, DateTime applicationDeterminedDate, string userName)
        {
            ApplicationId = applicationId;
            OversightStatus = oversightStatus;
            ApplicationDeterminedDate = applicationDeterminedDate;
            UserName = userName;
        }
    }
}
