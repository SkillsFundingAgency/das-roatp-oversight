using System;
using FluentValidation;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class OutcomePostRequestValidator : AbstractValidator<OutcomePostRequest>
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


        public OutcomePostRequestValidator()
        {
            RuleFor(x => x.ApproveGateway).NotEmpty().WithMessage("Select the gateway outcome for this application");
            
            RuleFor(x => x.ApproveModeration).NotEmpty()
                .WithMessage("Select the moderation outcome for this application");

            RuleFor(x => x.OversightStatus).NotEmpty()
                .WithMessage("Select the overall outcome of this application");
        }
    }
}
