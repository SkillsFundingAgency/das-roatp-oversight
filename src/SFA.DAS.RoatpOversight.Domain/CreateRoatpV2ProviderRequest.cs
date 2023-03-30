namespace SFA.DAS.RoatpOversight.Domain
{
    public class CreateRoatpV2ProviderRequest
    {
        public string Ukprn { get; set; }
        public string LegalName { get; set; }
        public string TradingName { get; set; }
        public string UserDisplayName { get; set; } 
        public string UserId { get; set; }
    }
}