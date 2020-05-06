﻿using Newtonsoft.Json;

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

        [JsonRequired] 
        public ClientApiAuthentication RoatpRegisterApiAuthentication { get; set; }

    }
}
