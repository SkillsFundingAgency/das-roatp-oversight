using System;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients.TokenService
{
    public interface ITokenService
    {
        string GetToken(Uri baseUri);
    }
}
