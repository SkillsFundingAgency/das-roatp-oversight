﻿using FluentValidation;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DASRoatpOversight.Web.Models;

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
        }

        public static class RuleSets
        {
            public const string Default = "Default";
            public const string GatewayOutcome = "GatewayOutcome";
        }
    }
}
