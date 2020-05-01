using SFA.DAS.RoatpOversight.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Infrastructure.ApiClients
{
    public interface IRoatpApplicationApiClient
    {
        Task<IEnumerable<ApplicationDetails>> GetOversightsPending();
        Task<IEnumerable<ApplicationDetails>> GetOversightsCompleted();
    }
}
