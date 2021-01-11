using System.Collections.Generic;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public static class OversightValidator
    {
        public const string MissingOverallOutcomeErrorMessage = "Select the overall outcome of this application";
        public const string MissingGatewayOutcomeErrorMessage = "Select the gateway outcome for this application";
        public const string MissingModerationOutcomeErrorMessage = "Select the moderation outcome for this application";

        public const string MissingOutcomeSuccessfulErrorMessage =
            "Select if you're sure this is a successful application";

        public const string MissingOutcomeUnsuccessfulErrorMessage =
            "Select if you're sure this is an unsuccessful application";

        public static List<ValidationErrorDetail> ValidateOverallOutcome(string outcomeStatus, string approveGateway, string approveModeration)
        {
            var errorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(approveGateway))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingGatewayOutcomeErrorMessage,
                    Field = "ApproveGateway"
                });
            }

            if (string.IsNullOrEmpty(approveModeration))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingModerationOutcomeErrorMessage,
                    Field = "ApproveModeration"
                });
            }

            if (string.IsNullOrEmpty(outcomeStatus))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingOverallOutcomeErrorMessage, Field = "Status"
                });
            }
            return errorMessages;
        }


        public static List<ValidationErrorDetail> ValidateOutcomeSuccessful(string outcomeStatus)
        {
            var errorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(outcomeStatus))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingOutcomeSuccessfulErrorMessage,
                    Field = "Status"
                });
            }
            return errorMessages;
        }


        public static List<ValidationErrorDetail> ValidateOutcomeUnsuccessful(string outcomeStatus)
        {
            var errorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(outcomeStatus))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingOutcomeUnsuccessfulErrorMessage,
                    Field = "Status"
                });
            }
            return errorMessages;
        }
    }
}
