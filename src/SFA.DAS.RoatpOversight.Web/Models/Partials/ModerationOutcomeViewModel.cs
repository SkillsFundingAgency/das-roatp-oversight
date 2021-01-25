using System;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class ModerationOutcomeViewModel
    {
        public string ModerationReviewStatus { get; set; }

        public DateTime? ModerationOutcomeMadeOn { get; set; }
        public string ModeratedBy { get; set; }
        public string ModerationComments { get; set; }
    }
}
