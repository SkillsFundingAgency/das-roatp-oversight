using System.Collections.Generic;

namespace SFA.DAS.RoatpOversight.Domain
{
    public static class AppealStatuses
    {
        public static IReadOnlyList<string> SuccessfulStatuses { get; } = new List<string>
        {
            AppealStatus.SuccessfulAlreadyActive,
            AppealStatus.Successful,
            AppealStatus.SuccessfulFitnessForFunding
        };
    }
}