using SFA.DAS.RoatpOversight.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Extensions
{
    public static class AppealStatusExtensions
    {
        public static string GetLabel(this string status)
        {
            switch (status)
            {
                case AppealStatus.Successful:
                case AppealStatus.SuccessfulAlreadyActive:
                case AppealStatus.SuccessfulFitnessForFunding:
                    return "Successful";
                case AppealStatus.InProgress:
                    return "In progress";
                case AppealStatus.Unsuccessful:
                    return "Unsuccessful";
                case AppealStatus.Submitted:
                    return "Submitted";
                default:
                    return "";
            }
        }       
        public static string GetCssClass(this string status)
        {
            switch (status)
            {
                case AppealStatus.Successful:
                case AppealStatus.SuccessfulAlreadyActive:
                case AppealStatus.SuccessfulFitnessForFunding:
                    return "govuk-tag das-tag--solid-green";
                case AppealStatus.Unsuccessful:
                    return "govuk-tag das-tag--solid-red";
                case AppealStatus.InProgress:
                case AppealStatus.Submitted:
                    return "govuk-tag das-tag";
                default:
                    return "";
            }
        }

    }
}
