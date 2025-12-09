using FluentValidation;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators;

public class AppealPostRequestValidator : AbstractValidator<AppealPostRequest>
{
    public AppealPostRequestValidator()
    {
        RuleFor(x => x.AppealStatus).NotEmpty()
            .WithMessage("Select the overall outcome of this appeal");

        RuleFor(x => x.InProgressInternalText).NotEmpty().WithMessage("Enter internal comments")
            .When(x => x.AppealStatus == AppealStatus.InProgress);

        RuleFor(x => x.InProgressExternalText).NotEmpty().WithMessage("Enter external comments")
            .When(x => x.AppealStatus == AppealStatus.InProgress);

        RuleFor(x => x.UnsuccessfulText).NotEmpty().WithMessage("Enter internal comments")
            .When(x => x.AppealStatus == AppealStatus.Unsuccessful);

        RuleFor(x => x.UnsuccessfulExternalText).NotEmpty().WithMessage("Enter external comments")
            .When(x => x.AppealStatus == AppealStatus.Unsuccessful);
    }
}
