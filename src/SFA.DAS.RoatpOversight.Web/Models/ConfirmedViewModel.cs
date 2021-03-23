using System;
using System.Linq;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmedViewModel
    {
        public Guid ApplicationId { get; set; }
        public OversightReviewStatus OversightStatus { get; set; }

        public OversightReviewStatus Status =>
            OversightReviewStatuses.SuccessfulStatuses.Contains(OversightStatus)
                ? OversightReviewStatus.Successful
                : OversightStatus;
    }
}
