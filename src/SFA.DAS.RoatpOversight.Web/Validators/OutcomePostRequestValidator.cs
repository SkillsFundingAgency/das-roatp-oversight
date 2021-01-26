using FluentValidation;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class OutcomePostRequestValidator : AbstractValidator<OutcomePostRequest>
    {
        public OutcomePostRequestValidator()
        {
            RuleFor(x => x.ApproveGateway).NotEmpty()
                .WithMessage("Select the gateway outcome for this application")
                .When(x => x.OversightStatus != OversightReviewStatus.InProgress);
            
            RuleFor(x => x.ApproveModeration).NotEmpty()
                .WithMessage("Select the moderation outcome for this application")
                .When(x => x.OversightStatus != OversightReviewStatus.InProgress);

            RuleFor(x => x.OversightStatus).NotEmpty()
                .WithMessage("Select the overall outcome of this application");

            RuleFor(x => x.InProgressInternalText).NotEmpty().WithMessage("Enter internal comments")
                .When(x => x.OversightStatus == OversightReviewStatus.InProgress);

            RuleFor(x => x.InProgressExternalText).NotEmpty().WithMessage("Enter external comments")
                .When(x => x.OversightStatus == OversightReviewStatus.InProgress);

            RuleFor(x => x.UnsuccessfulText).NotEmpty().WithMessage("Enter internal comments")
                .When(x => x.OversightStatus == OversightReviewStatus.Unsuccessful);

            RuleFor(x => x.UnsuccessfulExternalText).NotEmpty().WithMessage("Enter external comments")
                .When(x => x.OversightStatus == OversightReviewStatus.Unsuccessful);
        }
    }
}
