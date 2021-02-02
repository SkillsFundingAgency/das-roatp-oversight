using System.Collections.Generic;

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

    //todo: remove and obtain from shared package when available
    public enum OversightReviewStatus
    {
        None = 0,
        Successful = 1,
        SuccessfulAlreadyActive = 2,
        SuccessfulFitnessForFunding = 3,
        Unsuccessful = 4,
        InProgress = 5,
        Rejected = 6,
        Withdrawn = 7,
        Removed = 8
    }
}
