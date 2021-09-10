using FluentValidation;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class ConfirmAppealPostRequestValidator : AbstractValidator<ConfirmAppealPostRequest>
    {
        public ConfirmAppealPostRequestValidator()
        {
            RuleFor(x => x.Confirm).NotEmpty().WithMessage(GetConfirmationErrorMessage);
        }
    

    private string GetConfirmationErrorMessage(ConfirmAppealPostRequest arg)
        {
            var statusLabel = "";
            switch (arg.AppealStatus)
            {
                case AppealStatus.Successful:
                case AppealStatus.SuccessfulAlreadyActive:
                case AppealStatus.SuccessfulFitnessForFunding:
                    statusLabel = "successful";
                    break;
                case AppealStatus.InProgress:
                    statusLabel = "'in progress'";
                    break;
                case AppealStatus.Unsuccessful:
                    statusLabel = "unsuccessful";
                    break;
                default:
                    statusLabel = string.Empty;
                    break;
            }
            return $"Select if you want to mark this appeal as {statusLabel}";
        }
    }
}