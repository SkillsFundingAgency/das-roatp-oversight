using SFA.DAS.RoatpOversight.Domain;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IRoatpRegisterApiClient
    {
        Task<bool> CreateOrganisation(CreateRoatpOrganisationRequest organisationRequest);

        Task<OrganisationRegisterStatus> GetOrganisationRegisterStatus(GetOrganisationRegisterStatusRequest request);

        Task<bool> UpdateApplicationDeterminedDate(UpdateOrganisationApplicationDeterminedDateRequest request);


        Task<bool> UpdateOrganisation(UpdateOrganisationRequest request);
    }
}
