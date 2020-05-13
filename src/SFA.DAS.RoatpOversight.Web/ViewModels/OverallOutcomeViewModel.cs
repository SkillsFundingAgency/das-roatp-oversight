using System.Collections.Generic;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.ViewModels
{
    public class OverallOutcomeViewModel
    {
        public List<ApplicationDetails> ApplicationDetails { get; set; }
        public int ApplicationCount { get; set; }
        public List<OverallOutcomeDetails> OverallOutcomeDetails { get; set; }
        public int OverallOutcomeCount { get; set; }

    }
}
