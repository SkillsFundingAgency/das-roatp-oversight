using SFA.DAS.RoatpOversight.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients
{
    public interface IRoatpRegisterApiClient
    {
        Task<bool> CreateOrganisation(CreateRoatpOrganisationRequest organisationRequest);
    }
}
