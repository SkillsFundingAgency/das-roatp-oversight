﻿using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace SFA.DAS.RoatpOversight.Web.Domain
{
    public static class UserExtensions
    {
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
            var givenNameClaim = ClaimTypes.GivenName;
            var givenName = identity?.Claims.FirstOrDefault(x => x.Type == givenNameClaim);

            if (givenName == null)
            {
                Logger.LogError($"Unable to get value of claim {givenNameClaim}");
            }

            return givenName?.Value ?? "Unknown";
        }

        public static string Surname(this ClaimsPrincipal user)
        {
            var identity = user.Identities.FirstOrDefault();
            var surnameClaim = ClaimTypes.Surname;
            var surname = identity?.Claims.FirstOrDefault(x => x.Type == surnameClaim);
            if (surname == null)
            {
                Logger.LogError($"Unable to get value of claim {surnameClaim}");
            }

            return surname?.Value ?? "User";
        }
    }
}
