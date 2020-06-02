using SFA.DAS.AdminService.Common.Settings;

namespace SFA.DAS.RoatpOversight.Web.Settings
{
    public interface IWebConfiguration
    {
        string SessionRedisConnectionString { get; set; }

        AuthSettings StaffAuthentication { get; set; }
        ClientApiAuthentication ApplyApiAuthentication { get; set; }
        ClientApiAuthentication RoatpRegisterApiAuthentication { get; set; }
        string EsfaAdminServicesBaseUrl { get; set; }
    }
}
