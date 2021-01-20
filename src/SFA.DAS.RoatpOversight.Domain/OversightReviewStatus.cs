using System.Collections.Generic;

namespace SFA.DAS.RoatpOversight.Domain
{
    public static class OversightReviewStatus
    {
        public const string New = "New";
        public const string InProgress = "InProgress";
        public const string Successful = "Successful";
        public const string Unsuccessful = "Unsuccessful";
        public const string SuccessfulAlreadyActive = "SuccessfulAlreadyActive";
        public const string SuccessfulFitnessForFunding = "SuccessfulFitnessForFunding";

        public static IReadOnlyList<string> SuccessfulStatuses { get; } = new List<string>
        {
            OversightReviewStatus.SuccessfulAlreadyActive,
            OversightReviewStatus.Successful,
            OversightReviewStatus.SuccessfulFitnessForFunding
        };
    }
}
