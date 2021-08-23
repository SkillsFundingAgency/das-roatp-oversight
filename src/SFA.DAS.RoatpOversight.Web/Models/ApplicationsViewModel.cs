using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ApplicationsViewModel
    {
        public PendingOversightReviews ApplicationDetails { get; set; }
        public int ApplicationCount { get; set; }
        public CompletedOversightReviews OverallOutcomeDetails { get; set; }
        public int OverallOutcomeCount { get; set; }
        public AppealsOversightReviews AppealsDetails { get; set; }
        public int AppealsCount { get; set; }
        public AppealsOutcomeOversightReviews AppealsOutcomeDetails { get; set; }
        public int AppealsOutcomeCount { get; set; }

        public string SearchTerm { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public string SelectedTab { get; set; }

    }
}
