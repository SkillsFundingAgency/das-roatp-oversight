using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Infrastructure.ApiClients
{
    public class RoatpApplicationApiClient:  ApiClientBase<RoatpApplicationApiClient>, IRoatpApplicationApiClient
    {
        public RoatpApplicationApiClient(HttpClient httpClient, ILogger<RoatpApplicationApiClient> logger,, ITokenService tokenService) : base(httpClient, logger)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken());

        }

        public Task<IEnumerable<ApplicationDetails>> GetOversightsPending()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationDetails>> GetOversightsCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
