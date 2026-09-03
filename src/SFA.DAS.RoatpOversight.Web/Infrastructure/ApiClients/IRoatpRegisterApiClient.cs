using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Refit;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

public interface IRoatpRegisterApiClient
{
    [Post("/organisations")]
    Task<HttpResponseMessage> CreateOrganisation([Body] CreateRoatpOrganisationRequest organisationRequest);

    [Get("/organisations/{ukprn}")]
    Task<ApiResponse<Organisation>> GetOrganisation(int ukprn);

    [Put("/organisations/{ukprn}")]
    Task<HttpResponseMessage> UpdateOrganisation(int ukprn, [Body] UpdateOrganisationRequest request);

    [Put("/organisations/{ukprn}/course-types")]
    Task<HttpResponseMessage> UpdateCourseTypes(int ukprn, [Body] UpdateCourseTypesRequest request);

    [Patch("/organisations/{ukprn}")]
    Task<HttpResponseMessage> PatchOrganisation(int ukrpn, [Body] JsonPatchDocument<PatchOrganisationModel> patchDoc);
}
