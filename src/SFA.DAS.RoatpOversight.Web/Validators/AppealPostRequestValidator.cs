using FluentValidation;
using SFA.DAS.RoatpOversight.Domain.Extensions;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Services;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class AppealPostRequestValidator : AbstractValidator<AppealPostRequest>
    {
        private const int MaxFileSize = 1024 * 1024 * 5;

        public AppealPostRequestValidator(IPdfValidatorService pdfValidator)
        {
            RuleFor(x => x.Message)
                .NotEmpty()
                .WithMessage("Enter the applicant's appeal message")
                .When(x => x.SelectedOption == AppealPostRequest.SubmitOption.SaveAndContinue);

            RuleFor(x => x.FileUpload)
                .Configure(rule => rule.CascadeMode = CascadeMode.Stop)
                .NotNull().WithMessage("Select a file")
                .Must(x => x.Length > 0).WithMessage("The selected file is empty")
                .Must(x => x.Length <= MaxFileSize).WithMessage("The selected file must be smaller than 5MB")
                .MustAsync((file, token) => pdfValidator.IsPdf(file)).WithMessage("The selected file must be a PDF")
                .When(x => x.SelectedOption == AppealPostRequest.SubmitOption.Upload);
        }
    }
}
