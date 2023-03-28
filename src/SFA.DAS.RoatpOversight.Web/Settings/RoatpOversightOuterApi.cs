using SFA.DAS.RoatpOversight.Domain.Interfaces;

namespace SFA.DAS.RoatpOversight.Web.Settings
{
    public class RoatpOversightOuterApi: IRoatpOversightOuterApi
    {
        public string BaseUrl { get; set; } 
        public string SubscriptionKey { get; set; }
    }
}
