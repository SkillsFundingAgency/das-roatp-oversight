using FluentValidation;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class ConfirmAppealPostRequestValidator : AbstractValidator<ConfirmAppealPostRequest>
    {
        public ConfirmAppealPostRequestValidator()
        {
            RuleFor(x => x.Confirm).NotEmpty().WithMessage("None selected");
        }
    }
}