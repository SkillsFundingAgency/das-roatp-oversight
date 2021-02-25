using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Extensions
{
    public static class PassFailStatusExtensions
    {
        
        public static string GetLabel(this PassFailStatus status)
        {
            switch (status)
            {
                case PassFailStatus.Passed:
                    return "Passed";
                case PassFailStatus.Failed:
                    return "Failed";
                default:
                    return "";
            }
        }

        public static string GetCssClass(this PassFailStatus status)
        {
            switch (status)
            {
                case PassFailStatus.Passed:
                    return "govuk-tag das-tag--solid-green";
                case PassFailStatus.Failed:
                    return "govuk-tag das-tag--solid-red";
                default:
                    return "";
            }
        }
    }
}
