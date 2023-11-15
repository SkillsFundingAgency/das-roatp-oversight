using System.Security.Claims;
using SFA.DAS.DfESignIn.Auth.Enums;
using SFA.DAS.DfESignIn.Auth.Interfaces;

namespace SFA.DAS.RoatpOversight.Web.Settings
{
    public class CustomServiceRole : ICustomServiceRole
    {
        public string RoleClaimType => ClaimTypes.Role;
        public CustomServiceRoleValueType RoleValueType  => CustomServiceRoleValueType.Code;
    }
}