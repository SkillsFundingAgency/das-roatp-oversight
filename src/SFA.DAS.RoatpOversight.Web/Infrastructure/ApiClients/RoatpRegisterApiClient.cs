using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using SFA.DAS.RoatpOversight.Web.Settings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class RoatpRegisterApiClient : RoatpApiClientBase<RoatpRegisterApiClient>, IRoatpRegisterApiClient
    {
        public RoatpRegisterApiClient(ILogger<RoatpRegisterApiClient> logger, IRoatpRegisterTokenService tokenService, 
                                      IWebConfiguration configuration) 
        : base(configuration.RoatpRegisterApiAuthentication.ApiBaseAddress, logger, tokenService)
        {
        }
        public async Task<bool> CreateOrganisation(CreateRoatpOrganisationRequest organisationRequest)
        {
            HttpStatusCode result = await Post<CreateRoatpOrganisationRequest>($"/api/v1/organisation/create", organisationRequest);

            return await Task.FromResult(result == HttpStatusCode.OK);
        }

    }
}
