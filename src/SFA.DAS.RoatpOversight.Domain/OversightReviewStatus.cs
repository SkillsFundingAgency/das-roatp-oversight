using System.Collections.Generic;
using SFA.DAS.ApplyService.Types;

namespace SFA.DAS.RoatpOversight.Domain
{
    public static class OversightReviewStatuses
    {
        public static IReadOnlyList<OversightReviewStatus> SuccessfulStatuses { get; } = new List<OversightReviewStatus>
        {
            OversightReviewStatus.SuccessfulAlreadyActive,
            OversightReviewStatus.Successful,
            OversightReviewStatus.SuccessfulFitnessForFunding
        };
    }

}
