using System.Collections.Generic;

namespace SFA.DAS.RoatpOversight.Domain
{
    public static class OversightReviewStatus
    {
        public const string InProgress = "In progress";
        public const string Successful = "Successful";
        public const string Unsuccessful = "Unsuccessful";
        public const string SuccessfulAlreadyActive = "Successful - already active";
        public const string SuccessfulFitnessForFunding = "Successful - fitness for funding";

        public static IReadOnlyList<string> SuccessfulStatuses { get; } = new List<string>
        {
            OversightReviewStatus.SuccessfulAlreadyActive,
            OversightReviewStatus.Successful,
            OversightReviewStatus.SuccessfulFitnessForFunding
        };
    }
}
