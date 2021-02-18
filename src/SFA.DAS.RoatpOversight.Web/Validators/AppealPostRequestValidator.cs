using FluentValidation;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class AppealPostRequestValidator : AbstractValidator<AppealPostRequest>
    {
        public AppealPostRequestValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Enter the applicant's appeal message")
                .When(x => x.SelectedOption == AppealPostRequest.SubmitOption.SaveAndContinue);

            RuleFor(x => x.FileUpload)
                .NotNull()
                .WithMessage("Select a file")
                .When(x => x.SelectedOption == AppealPostRequest.SubmitOption.Upload);
        }
    }
}
