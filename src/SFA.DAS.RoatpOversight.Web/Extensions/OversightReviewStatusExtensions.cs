using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Extensions
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

        public static string GetSpecificLabel(this OversightReviewStatus status)
        {
            switch (status)
            {
                case OversightReviewStatus.Successful:
                    return "Successful";
                case OversightReviewStatus.SuccessfulAlreadyActive:
                    return "Successful - already active";
                case OversightReviewStatus.SuccessfulFitnessForFunding:
                    return "Successful - fitness for funding";
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
                    return "govuk-tag das-tag--solid-green";
                case OversightReviewStatus.InProgress:
                    return "govuk-tag das-tag";
                case OversightReviewStatus.Rejected:
                    return "govuk-tag das-tag--solid-brown";
                case OversightReviewStatus.Removed:
                    return "govuk-tag govuk-tag--inactive";
                case OversightReviewStatus.Unsuccessful:
                    return "govuk-tag das-tag--solid-red";
                case OversightReviewStatus.Withdrawn:
                    return "govuk-tag govuk-tag--inactive";
                default:
                    return "";
            }
        }

    }
}
