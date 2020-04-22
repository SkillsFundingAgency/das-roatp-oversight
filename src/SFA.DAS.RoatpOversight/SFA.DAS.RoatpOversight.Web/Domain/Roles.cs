﻿using System.Security.Claims;

namespace SFA.DAS.RoatpOversight.Web.Domain
{
    public static class Roles
    {
        public const string RoleClaimType = "http://service/service";

        public const string RoatpGatewayTeam = "APR";
        public const string RoatpGatewayAssessorTeam = "GAC";
        public const string RoatpFinancialAssessorTeam = "FHC"; // RoATP FHA
        public const string RoatpApplicationOversightTeam = "AOV";
        public const string RoatpOversightTeam = "AAC";

        public static bool HasValidRole(this ClaimsPrincipal user)
        {
            return user.IsInRole(RoatpGatewayTeam)
                   || user.IsInRole(RoatpGatewayAssessorTeam)
                   || user.IsInRole(RoatpFinancialAssessorTeam)
                   || user.IsInRole(RoatpOversightTeam)
                   || user.IsInRole(RoatpApplicationOversightTeam);
        }
    }
}
