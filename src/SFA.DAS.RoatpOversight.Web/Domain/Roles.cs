using System.Security.Claims;

namespace SFA.DAS.RoatpOversight.Web.Domain
{
    public static class Roles
    {
        public const string RoleClaimType = "http://service/service";

        public const string RoatpApplicationOversightTeam = "AOV";

        public static bool HasValidRole(this ClaimsPrincipal user)
        {
            return user.IsInRole(RoatpApplicationOversightTeam);
        }
    }
}
