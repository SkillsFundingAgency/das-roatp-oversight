using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

public interface IRoatpRegisterApiClient
{
    [Post("/organisations")]
    Task<bool> CreateOrganisation([Body] CreateRoatpOrganisationRequest organisationRequest);

    [Get("/organisations/{ukprn}")]
    Task<ApiResponse<Organisation>> GetOrganisation(int ukprn);

    [Put("/organisations/{ukprn}")]
    Task<HttpResponseMessage> UpdateOrganisation(int ukprn, [Body] UpdateOrganisationRequest request);
}
