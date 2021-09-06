using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealViewModel: OutcomeViewModel
    {
        public AppealDetails Appeal { get; set; }
        public string AppealStatus { get; set; }
    }
}