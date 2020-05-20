using NUnit.Framework;
using SFA.DAS.AdminService.Common.Testing.MockedObjects;
using SFA.DAS.RoatpOversight.Web.Domain;
using System;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Domain
{
    [TestFixture]
    public class RolesTests
    {
        [Test]
        public void When_invalid_role_HasValidRole_returns_False()
        {
            var knownInvalidRole = Guid.Empty.ToString("n");
            var user = MockedUser.Setup(knownInvalidRole);

            var actualResult = Roles.HasValidRole(user);

            Assert.That(actualResult, Is.False);
        }

        [Test]
        public void When_valid_role__HasValidRole_returns_True()
        {
            var knownValidRole = Roles.RoatpApplicationOversightTeam;
            var user = MockedUser.Setup(knownValidRole);

            var actualResult = Roles.HasValidRole(user);

            Assert.That(actualResult, Is.True);
        }
    }
}
