using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

namespace SFA.DAS.RoatpOversight.Web.HealthChecks
{
    public class ApplyApiHealthCheck : IHealthCheck
    {
        private readonly IApplyApiClient _apiClient;

        public ApplyApiHealthCheck(IApplyApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                await _apiClient.Ping();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
