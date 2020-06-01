
using System;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService
{
    public interface IRoatpRegisterTokenService
    {
        string GetToken(Uri baseUri);
    }
}
