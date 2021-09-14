using System;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public class AppealValidatorInterceptor : IValidatorInterceptor
    {
        public IValidationContext BeforeMvcValidation(ControllerContext controllerContext, IValidationContext commonContext)
        {
            if (!(commonContext.InstanceToValidate is AppealPostRequest target))
            {
                throw new InvalidOperationException(
                    $"AppealValidatorInterceptor cannot be used with type {commonContext.InstanceToValidate.GetType().Name}");
            }

            return new ValidationContext<AppealPostRequest>(target,
                null,
                new RulesetValidatorSelector(AppealPostRequestValidator.RuleSets.Default));
        }

        public ValidationResult AfterMvcValidation(ControllerContext controllerContext, IValidationContext commonContext,
            ValidationResult result)
        {
            return result;
        }
    }
}