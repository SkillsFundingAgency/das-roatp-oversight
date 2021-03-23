using FluentValidation;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class ConfirmOutcomePostRequestValidator : AbstractValidator<ConfirmOutcomePostRequest>
    {
        public ConfirmOutcomePostRequestValidator()
        {
            RuleFor(x => x.Confirm).NotEmpty().WithMessage(GetConfirmationErrorMessage);
        }

        private string GetConfirmationErrorMessage(ConfirmOutcomePostRequest arg)
        {
            var statusLabel = "";
            switch (arg.OversightStatus)
            {
                case OversightReviewStatus.Successful:
                case OversightReviewStatus.SuccessfulAlreadyActive:
                case OversightReviewStatus.SuccessfulFitnessForFunding:
                    statusLabel = "successful";
                    break;
                case OversightReviewStatus.InProgress:
                    statusLabel = "'in progress'";
                    break;
                case OversightReviewStatus.Unsuccessful:
                    statusLabel = "unsuccessful";
                    break;
                default:
                    statusLabel = string.Empty;
                    break;
            }
            return $"Select whether you are sure you want to mark this application as {statusLabel}";
        }
    }
}
