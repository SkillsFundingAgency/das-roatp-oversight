using System;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class FinancialHealthOutcomeViewModel
    {
        public string FinancialReviewStatus { get; set; }
        public string FinancialGradeAwarded { get; set; }

        public DateTime? FinancialHealthAssessedOn { get; set; }
        public string FinancialHealthAssessedBy { get; set; }
        public string FinancialHealthComments { get; set; }
    }
}
