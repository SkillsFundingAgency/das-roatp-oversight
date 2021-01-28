using System;
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
            if(!(commonContext.InstanceToValidate is OutcomePostRequest target))
            {
                throw new InvalidOperationException(
                    $"OutcomeValidatorInterceptor cannot be used with type {commonContext.InstanceToValidate.GetType().Name}");
            }

            if(!target.IsGatewayFail)
            {
                return new ValidationContext<OutcomePostRequest>(target,
                    null,
                    new RulesetValidatorSelector(OutcomePostRequestValidator.RuleSets.Default));
            }

            return new ValidationContext<OutcomePostRequest>(target, null,
                new RulesetValidatorSelector(OutcomePostRequestValidator.RuleSets.GatewayFail));
        }

        public ValidationResult AfterMvcValidation(ControllerContext controllerContext, IValidationContext commonContext,
            ValidationResult result)
        {
            return result;
        }
    }
}
