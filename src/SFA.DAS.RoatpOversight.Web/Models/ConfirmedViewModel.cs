using System;
using System.Linq;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmedViewModel
    {
        public Guid ApplicationId { get; set; }
        public string OversightStatus { get; set; }

        public string Status =>
            OversightReviewStatus.SuccessfulStatuses.Contains(OversightStatus)
                ? OversightReviewStatus.Successful
                : OversightStatus;
    }
}
