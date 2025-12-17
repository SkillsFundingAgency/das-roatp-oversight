namespace SFA.DAS.RoatpOversight.Web.Settings;

public interface IWebConfiguration
{
    string SessionRedisConnectionString { get; set; }
    string SessionCachingDatabase { get; set; }
    string DataProtectionKeysDatabase { get; set; }

    ManagedIdentityApiAuthentication ApplyApiAuthentication { get; set; }
    ManagedIdentityApiAuthentication RoatpRegisterApiAuthentication { get; set; }
    RoatpOversightOuterApi RoatpOversightOuterApi { get; set; }
    string EsfaAdminServicesBaseUrl { get; set; }
    bool UseDfeSignIn { get; set; }
    string DfESignInServiceHelpUrl { get; set; }
}
