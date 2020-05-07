using System.Collections.Generic;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Validators
{
    public static class OverallOutcomeValidator
    {
        public const string MissingOverallOutcomeErrorMessage = "Select the overall outcome of this application";

        public static List<ValidationErrorDetail> ValidateOverallOutcome(string outcomeStatus)
        {
            var errorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(outcomeStatus))
            {
                errorMessages.Add(new ValidationErrorDetail
                {
                    ErrorMessage = MissingOverallOutcomeErrorMessage, Field = "Status"
                });
            }
            return errorMessages;
        }
    }
}
