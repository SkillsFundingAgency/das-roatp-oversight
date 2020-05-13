using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightSuccessfulValidatorTests
    {
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("yes", false)]
        [TestCase("no", false)]
        [TestCase("YES", false)]
        [TestCase("NO", false)]
        public void OversightSuccessfulValidator_returns_error_when_status_is_empty(string status, bool errorsExpected)
        {
            var validationDetails = OversightValidator.ValidateOutcomeSuccessful(status);
            Assert.AreEqual(errorsExpected,validationDetails.Count>0);
        }

        [Test]
        public void OversightSuccessfulValidator_returns_expected_error_message_when_status_is_empty()
        {
            var validationDetails = OversightValidator.ValidateOutcomeSuccessful(string.Empty);
            Assert.AreEqual(OversightValidator.MissingOutcomeSuccessfulErrorMessage, validationDetails.First().ErrorMessage);
        }
    }
}
