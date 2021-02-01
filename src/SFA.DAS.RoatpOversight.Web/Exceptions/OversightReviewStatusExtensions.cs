﻿using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Exceptions
{
    public static class OversightReviewStatusExtensions
    {
        public static string GetLabel(this OversightReviewStatus status)
        {
            switch (status)
            {
                case OversightReviewStatus.Successful:
                case OversightReviewStatus.SuccessfulAlreadyActive:
                case OversightReviewStatus.SuccessfulFitnessForFunding:
                    return "Successful";
                case OversightReviewStatus.InProgress:
                    return "In progress";
                case OversightReviewStatus.Rejected:
                    return "Rejected";
                case OversightReviewStatus.Removed:
                    return "Removed";
                case OversightReviewStatus.Unsuccessful:
                    return "Unsuccessful";
                case OversightReviewStatus.Withdrawn:
                    return "Withdrawn";
                default:
                    return "";
            }
        }

        public static string GetCssClass(this OversightReviewStatus status)
        {
            switch (status)
            {
                case OversightReviewStatus.Successful:
                case OversightReviewStatus.SuccessfulAlreadyActive:
                case OversightReviewStatus.SuccessfulFitnessForFunding:
                    return "das-tag--solid-green";
                case OversightReviewStatus.InProgress:
                    return "das-tag--solid-blue";
                case OversightReviewStatus.Rejected:
                    return "das-tag--solid-red";
                case OversightReviewStatus.Removed:
                    return "das-tag--solid-grey";
                case OversightReviewStatus.Unsuccessful:
                    return "das-tag--solid-red";
                case OversightReviewStatus.Withdrawn:
                    return "das-tag--solid-grey";
                default:
                    return "";
            }
        }

    }
}
