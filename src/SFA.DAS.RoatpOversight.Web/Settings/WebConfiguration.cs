using Newtonsoft.Json;

namespace SFA.DAS.RoatpOversight.Web.Settings;

public class WebConfiguration : IWebConfiguration
{
    [JsonRequired]
    public string SessionRedisConnectionString { get; set; }

    [JsonRequired]
    public string SessionCachingDatabase { get; set; }

    [JsonRequired]
    public string DataProtectionKeysDatabase { get; set; }

    [JsonRequired]
    public ManagedIdentityApiAuthentication ApplyApiAuthentication { get; set; }

    [JsonRequired]
    public ManagedIdentityApiAuthentication RoatpRegisterApiAuthentication { get; set; }

    [JsonRequired]
    public string EsfaAdminServicesBaseUrl { get; set; }

    [JsonRequired]
    public RoatpOversightOuterApi RoatpOversightOuterApi { get; set; }

    public string DfESignInServiceHelpUrl { get; set; }
}
