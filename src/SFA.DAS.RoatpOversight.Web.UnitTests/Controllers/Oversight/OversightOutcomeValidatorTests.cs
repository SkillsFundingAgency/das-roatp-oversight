using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightOutcomeValidatorTests
    {
        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase(OversightReviewStatus.Successful, false)]
        [TestCase(OversightReviewStatus.Unsuccessful, false)]
        public void OversightOutcomeValidator_returns_error_when_status_is_empty(string status, bool errorsExpected)
        {
            var validationDetails = OverallOutcomeValidator.ValidateOverallOutcome(status);
            Assert.AreEqual(errorsExpected,validationDetails.Count>0);
        }

        [Test]
        public void OversightOutcomeValidator_returns_expected_error_message_when_status_is_empty()
        {
            var validationDetails = OverallOutcomeValidator.ValidateOverallOutcome(string.Empty);
            Assert.AreEqual(OverallOutcomeValidator.MissingOverallOutcomeErrorMessage, validationDetails.First().ErrorMessage);
        }
    }
}
