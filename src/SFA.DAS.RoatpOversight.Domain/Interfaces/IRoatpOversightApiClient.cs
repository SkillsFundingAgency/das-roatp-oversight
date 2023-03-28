using System.Threading.Tasks;
using RestEase;

namespace SFA.DAS.RoatpOversight.Domain.Interfaces
{
    public interface IRoatpOversightApiClient
    {
        [Post("/providers")]
        Task CreateProvider([Body]CreateRoatpV2ProviderRequest providerRequest);
    }
}