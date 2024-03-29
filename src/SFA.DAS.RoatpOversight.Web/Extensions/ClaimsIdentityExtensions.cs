using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.RoatpOversight.Web.Extensions
{
    public static class ClaimsIdentityExtensions
    {
        private const string _unknownGivenName = "Unknown";
        private const string _unknownSurname = "Unknown";

        static public ILogger<ClaimsPrincipal> Logger { set; get; }

        public static string UserDisplayName(this ClaimsPrincipal user)
        {
            var givenNameClaim = user.GivenName();
            var surnameClaim = user.Surname();

            return $"{givenNameClaim} {surnameClaim}";
        }

        public static string GivenName(this ClaimsPrincipal user)
        {
            var identity = user.Identities.FirstOrDefault();
            var givenNameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
            var givenName = identity?.Claims.FirstOrDefault(x => x.Type == givenNameClaim || x.Type == "given_name");

            if (givenName == null)
            {
                Logger.LogError($"Unable to get value of claim {givenNameClaim}");
            }

            return givenName?.Value ?? "Unknown";
        }

        public static string Surname(this ClaimsPrincipal user)
        {
            var identity = user.Identities.FirstOrDefault();
            var surnameClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
            var surname = identity?.Claims.FirstOrDefault(x => x.Type == surnameClaim || x.Type == "family_name");
            if (surname == null)
            {
                Logger.LogError($"Unable to get value of claim {surnameClaim}");
            }

            return surname?.Value ?? "User";
        }

        public static string UserId(this ClaimsPrincipal user)
        {
            var upnClaimName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
            var upn = user.Identities.FirstOrDefault().Claims.FirstOrDefault(x => x.Type == upnClaimName || x.Type == "email");
            return upn?.Value ?? $"{_unknownGivenName}-{_unknownSurname}";
        }
    }
}