using System;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class OverallOutcomeViewModel
    {
        public OversightReviewStatus OversightStatus { get; set; }
        public DateTime? ApplicationDeterminedDate { get; set; }
        public string OversightUserName { get; set; }
    }
}
