using System;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class GatewayOutcomeViewModel
    {
        public string GatewayReviewStatus { get; set; }
        public DateTime? GatewayOutcomeMadeDate { get; set; }

        public string GatewayOutcomeMadeBy { get; set; }
        public string GatewayComments { get; set; }
        public string GatewayExternalComments { get; set; }
        public PassFailStatus GovernanceOutcome { get; set; }
    }
}
