namespace SFA.DAS.RoatpOversight.Domain.Interfaces
{
    public interface IRoatpOversightOuterApi
    {
        string BaseUrl { get; set; }
        string SubscriptionKey { get; set; }
    }
}
