using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IApplyApiClient
    {
        Task<IEnumerable<ApplicationDetails>> GetOversightsPending();
        Task<IEnumerable<OverallOutcomeDetails>> GetOversightsCompleted();
    }
}
