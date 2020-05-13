using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightUnsuccessfulValidatorTests
    {
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("yes", false)]
        [TestCase("no", false)]
        [TestCase("YES", false)]
        [TestCase("NO", false)]
        public void OversightUnsuccessfulValidator_returns_error_when_status_is_empty(string status, bool errorsExpected)
        {
            var validationDetails = OversightValidator.ValidateOutcomeUnsuccessful(status);
            Assert.AreEqual(errorsExpected,validationDetails.Count>0);
        }

        [Test]
        public void OversightUnsuccessfulValidator_returns_expected_error_message_when_status_is_empty()
        {
            var validationDetails = OversightValidator.ValidateOutcomeUnsuccessful(string.Empty);
            Assert.AreEqual(OversightValidator.MissingOutcomeUnsuccessfulErrorMessage, validationDetails.First().ErrorMessage);
        }
    }
}
