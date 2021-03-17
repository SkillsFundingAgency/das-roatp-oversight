using FluentValidation;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class OutcomePostRequestValidator : AbstractValidator<OutcomePostRequest>
    {
        public OutcomePostRequestValidator()
        {
            RuleSet(RuleSets.Default, () =>
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
                    .When(x => x.OversightStatus == OversightReviewStatus.Unsuccessful)
                    .When(x => x.ApproveGateway == ApprovalStatus.Overturn || x.ApproveModeration == ApprovalStatus.Overturn);

            });

            RuleSet(RuleSets.GatewayOutcome, () =>
            {
                //no rules
            });

            RuleSet(RuleSets.AppealOutcome, () =>
                {
                    RuleFor(x => x.SelectedAppealStatus).NotEmpty().WithMessage("Select the outcome of this appeal");

                    RuleFor(x => x.SuccessfulText).NotEmpty().WithMessage("Enter internal comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.Successful);

                    RuleFor(x => x.SuccessfulExternalText).NotEmpty().WithMessage("Enter external comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.Successful);

                    RuleFor(x => x.SuccessfulAlreadyActiveText).NotEmpty().WithMessage("Enter internal comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.SuccessfulAlreadyActive);

                    RuleFor(x => x.SuccessfulAlreadyActiveExternalText).NotEmpty().WithMessage("Enter external comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.SuccessfulAlreadyActive);

                    RuleFor(x => x.UnsuccessfulText).NotEmpty().WithMessage("Enter internal comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.Unsuccessful);

                    RuleFor(x => x.UnsuccessfulExternalText).NotEmpty().WithMessage("Enter external comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.Unsuccessful);

                    RuleFor(x => x.UnsuccessfulPartiallyUpheldText).NotEmpty().WithMessage("Enter internal comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.UnsuccessfulPartiallyUpheld);

                    RuleFor(x => x.UnsuccessfulPartiallyUpheldExternalText).NotEmpty().WithMessage("Enter external comments")
                        .When(x => x.SelectedAppealStatus == AppealStatus.UnsuccessfulPartiallyUpheld);
                });
        }

        public static class RuleSets
        {
            public const string Default = "Default";
            public const string GatewayOutcome = "GatewayOutcome";
            public const string AppealOutcome = "AppealOutcome";
        }
    }
}
