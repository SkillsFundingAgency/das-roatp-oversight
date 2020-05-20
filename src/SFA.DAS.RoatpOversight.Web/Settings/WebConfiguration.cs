using Newtonsoft.Json;
using SFA.DAS.AdminService.Common.Settings;

namespace SFA.DAS.RoatpOversight.Web.Settings
{
    public class WebConfiguration : IWebConfiguration
    {
        [JsonRequired]
        public string SessionRedisConnectionString { get; set; }

        [JsonRequired]
        public AuthSettings StaffAuthentication { get; set; }

        [JsonRequired]
        public ClientApiAuthentication ApplyApiAuthentication { get; set; }
    }
}
