using System.Threading.Tasks;
using Refit;

namespace SFA.DAS.RoatpOversight.Domain.Interfaces;

public interface IRoatpOversightOuterApiClient
{
    [Post("/providers")]
    Task CreateProvider([Body] CreateRoatpV2ProviderRequest providerRequest);

    [Post("/providers/course-types")]
    Task AssignCourseTypesToProvider([Body] AssignCourseTypesToProviderRequest request);
}
