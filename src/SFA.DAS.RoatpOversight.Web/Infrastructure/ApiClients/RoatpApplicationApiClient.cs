﻿using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService;
using SFA.DAS.AdminService.Common.Infrastructure;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public class RoatpApplicationApiClient : ApiClientBase<RoatpApplicationApiClient>, IRoatpApplicationApiClient
    {
        public RoatpApplicationApiClient(HttpClient httpClient, ILogger<RoatpApplicationApiClient> logger, IRoatpApplicationTokenService tokenService) : base(httpClient, logger)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.GetToken(_httpClient.BaseAddress));
        }

        // Add end points below
    }
}
