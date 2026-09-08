using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Refit;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;

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
    Task<HttpResponseMessage> PatchOrganisation(int ukprn, [Header(RequestHeaders.RequestingUserIdHeader)] string userId, [Header(RequestHeaders.RequestingUserNameHeader)] string userName, [Body] JsonPatchDocument<PatchOrganisationModel> patchDoc);
}
