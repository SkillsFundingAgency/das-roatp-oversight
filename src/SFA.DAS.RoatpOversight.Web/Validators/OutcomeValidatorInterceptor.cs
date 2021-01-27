using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class OutcomeValidatorInterceptor : IValidatorInterceptor
    {
        public IValidationContext BeforeMvcValidation(ControllerContext controllerContext, IValidationContext commonContext)
        {
            var target = commonContext.InstanceToValidate as OutcomePostRequest;

            if(!target.IsGatewayFail)
            {
                return new ValidationContext<OutcomePostRequest>(target,
                    null,
                    new RulesetValidatorSelector("DefaultRuleset"));
            }

            return new ValidationContext<OutcomePostRequest>(target, null,
                new RulesetValidatorSelector("GatewayFailRuleset"));
        }

        public ValidationResult AfterMvcValidation(ControllerContext controllerContext, IValidationContext commonContext,
            ValidationResult result)
        {
            return result;
        }
    }
}
