using System;
using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure;
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
        [TestCase(OversightReviewStatus.Successful, "", ModerationReviewStatus.Pass, true)]
        [TestCase(OversightReviewStatus.Unsuccessful, null, ModerationReviewStatus.Pass, true)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Pass, "", true)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Pass, null, true)]
        public void OversightOutcomeValidator_returns_error_when_status_is_empty(string oversightStatus, string approveGateway, string approveModeration, bool errorsExpected)
        {
            var command = new EvaluationOutcomeCommand
            {
                OversightStatus = oversightStatus,
                ApproveGateway = approveGateway,
                ApproveModeration = approveModeration,
                UnsuccessfulText = oversightStatus == OversightReviewStatus.Unsuccessful ? "comments" : null
            };
            var validationDetails = OversightValidator.ValidateOverallOutcome(command);
            Assert.AreEqual(errorsExpected,validationDetails.Count>0);
        }

        [TestCase(null,null,2)]
        [TestCase(null, "", 2)]
        [TestCase("", null, 2)]
        [TestCase("a", null, 1)]
        [TestCase(null, "a", 1)]
        [TestCase("a", "b", 0)]
        public void OversightOutcomeValidator_in_progress_and_comments_set_returns_expected_result(string internalComments, string externalComments, int numberOfErrorsExpected)
        {
            var command = new EvaluationOutcomeCommand
            {
                OversightStatus = OversightReviewStatus.InProgress,
                InProgressInternalText = internalComments,
                InProgressExternalText = externalComments
            };

            var validationDetails = OversightValidator.ValidateOverallOutcome(command);
            Assert.AreEqual(numberOfErrorsExpected, validationDetails.Count);
        }

        [TestCase("")]
        [TestCase(null)]
        public void OversightOutcomeValidator_returns_expected_error_message_when_status_is_empty(string oversightStatus)
        {
            var command = new EvaluationOutcomeCommand
            {
                OversightStatus = oversightStatus,
                ApproveGateway = GatewayReviewStatus.Pass,
                ApproveModeration = ModerationReviewStatus.Pass
            };
            var validationDetails = OversightValidator.ValidateOverallOutcome(command);
            Assert.AreEqual(OversightValidator.MissingOverallOutcomeErrorMessage, validationDetails.First().ErrorMessage);
        }
    }
}
