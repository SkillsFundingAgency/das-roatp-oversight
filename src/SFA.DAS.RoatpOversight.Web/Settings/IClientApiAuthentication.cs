namespace SFA.DAS.RoatpOversight.Web.Settings
{
    public interface IClientApiAuthentication
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string ResourceId { get; set; }
        string TenantId { get; set; }
        string ApiBaseAddress { get; set; }
    }
}