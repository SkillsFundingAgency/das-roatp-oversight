using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IRoatpOversightApiClient
    {
        Task<bool> CreateProvider(CreateRoatpV2ProviderRequest providerRequest);
    }
}