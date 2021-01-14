using System.Collections.Generic;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure;

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

        public const string EnterInternalComments = "Enter internal comments";
        public const string EnterExternalComments = "Enter external comments";


        public static List<ValidationErrorDetail> ValidateOverallOutcome(EvaluationOutcomeCommand command)
        {
            var errorMessages = new List<ValidationErrorDetail>();

            if (command.OversightStatus == OversightReviewStatus.InProgress && (string.IsNullOrEmpty(command.ApproveGateway) || string.IsNullOrEmpty(command.ApproveModeration)))
            {
                if (string.IsNullOrEmpty(command.InProgressInternalText))
                    {
                        errorMessages.Add(new ValidationErrorDetail
                        {
                            ErrorMessage = EnterInternalComments,
                            Field = "InProgressInternalText"
                        });
                    }

                    if (string.IsNullOrEmpty(command.InProgressExternalText))
                    {
                        errorMessages.Add(new ValidationErrorDetail
                        {
                            ErrorMessage = EnterExternalComments,
                            Field = "InProgressExternalText"
                        });
                    }

                    return errorMessages;
            }

            if (string.IsNullOrEmpty(command.ApproveGateway))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingGatewayOutcomeErrorMessage,
                    Field = "ApproveGateway"
                });
            }

            if (string.IsNullOrEmpty(command.ApproveModeration))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingModerationOutcomeErrorMessage,
                    Field = "ApproveModeration"
                });
            }

            if (string.IsNullOrEmpty(command.OversightStatus))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingOverallOutcomeErrorMessage, Field = "Status"
                });
            }

            if (command.OversightStatus == OversightReviewStatus.Unsuccessful  && string.IsNullOrEmpty(command.UnsuccessfulText))
            {
                if (string.IsNullOrEmpty(command.UnsuccessfulText))
                {
                    errorMessages.Add(new ValidationErrorDetail
                    {
                        ErrorMessage = EnterInternalComments,
                        Field = "UnsuccessfulText"
                    });
                }
            }

            if (command.OversightStatus == OversightReviewStatus.InProgress)
            {
                if (string.IsNullOrEmpty(command.InProgressInternalText))
                {
                    errorMessages.Add(new ValidationErrorDetail
                    {
                        ErrorMessage = EnterInternalComments,
                        Field = "InProgressInternalText"
                    });
                }

                if (string.IsNullOrEmpty(command.InProgressExternalText))
                {
                    errorMessages.Add(new ValidationErrorDetail
                    {
                        ErrorMessage = EnterExternalComments,
                        Field = "InProgressExternalText"
                    });
                }
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
