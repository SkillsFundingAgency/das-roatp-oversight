using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OversightOutcomeValidatorTests
    {
        [TestCase("", GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, true)]
        [TestCase(null, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, true)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, false)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, false)]
        public void OversightOutcomeValidator_returns_error_when_status_is_empty(string oversightStatus, string approveGateway, string approveModeration, bool errorsExpected)
        {
            var validationDetails = OversightValidator.ValidateOverallOutcome(oversightStatus, approveGateway, approveModeration);
            Assert.AreEqual(errorsExpected,validationDetails.Count>0);
        }

        [TestCase("")]
        [TestCase(null)]
        public void OversightOutcomeValidator_returns_expected_error_message_when_status_is_empty(string oversightStatus)
        {
            var validationDetails = OversightValidator.ValidateOverallOutcome(oversightStatus, GatewayReviewStatus.Pass,  ModerationReviewStatus.Pass);
            Assert.AreEqual(OversightValidator.MissingOverallOutcomeErrorMessage, validationDetails.First().ErrorMessage);
        }
    }
}
