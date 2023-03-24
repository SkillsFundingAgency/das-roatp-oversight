using SFA.DAS.AdminService.Common.Settings;

namespace SFA.DAS.RoatpOversight.Web.Settings
{
    public interface IWebConfiguration
    {
        string SessionRedisConnectionString { get; set; }
        string SessionCachingDatabase { get; set; }
        string DataProtectionKeysDatabase { get; set; }

        AuthSettings StaffAuthentication { get; set; }
        ManagedIdentityApiAuthentication ApplyApiAuthentication { get; set; }
        ClientApiAuthentication RoatpRegisterApiAuthentication { get; set; }
        RoatpOversightOuterApi RoatpOversightOuterApi { get; set; }
        string EsfaAdminServicesBaseUrl { get; set; }
    }
}
