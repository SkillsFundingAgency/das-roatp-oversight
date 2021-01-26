using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OutcomePostRequestValidatorTests
    {
        private OutcomePostRequestValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new OutcomePostRequestValidator();
        }

        [TestCase(null, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, true)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, false)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, false)]
        [TestCase(OversightReviewStatus.Successful, "", ModerationReviewStatus.Pass, true)]
        [TestCase(OversightReviewStatus.Unsuccessful, null, ModerationReviewStatus.Pass, true)]
        [TestCase(OversightReviewStatus.Successful, GatewayReviewStatus.Pass, "", true)]
        [TestCase(OversightReviewStatus.Unsuccessful, GatewayReviewStatus.Pass, null, true)]
        public void OversightOutcomeValidator_returns_error_when_status_is_empty(OversightReviewStatus oversightStatus, string approveGateway, string approveModeration, bool errorsExpected)
        {
            var request = new OutcomePostRequest
            {
                OversightStatus = oversightStatus,
                ApproveGateway = approveGateway,
                ApproveModeration = approveModeration,
                UnsuccessfulText = oversightStatus == OversightReviewStatus.Unsuccessful ? "comments" : null
            };

            var result = _validator.Validate(request);
            Assert.AreEqual(errorsExpected,!result.IsValid);
        }

        [TestCase(null,null,2)]
        [TestCase(null, "", 2)]
        [TestCase("", null, 2)]
        [TestCase("a", null, 1)]
        [TestCase(null, "a", 1)]
        [TestCase("a", "b", 0)]
        public void OversightOutcomeValidator_in_progress_and_comments_set_returns_expected_result(string internalComments, string externalComments, int numberOfErrorsExpected)
        {
            var request = new OutcomePostRequest
            {
                ApproveGateway = GatewayReviewStatus.Pass,
                ApproveModeration = ModerationReviewStatus.Pass,
                OversightStatus = OversightReviewStatus.InProgress,
                InProgressInternalText = internalComments,
                InProgressExternalText = externalComments
            };

            var result = _validator.Validate(request);
            Assert.AreEqual(numberOfErrorsExpected, result.Errors.Count);
        }

        [TestCase(OversightReviewStatus.None)]
        public void OversightOutcomeValidator_returns_expected_error_message_when_status_is_empty(OversightReviewStatus oversightStatus)
        {
            var request = new OutcomePostRequest
            {
                OversightStatus = oversightStatus,
                ApproveGateway = GatewayReviewStatus.Pass,
                ApproveModeration = ModerationReviewStatus.Pass
            };
            var result = _validator.Validate(request);

            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == "OversightStatus" && x.ErrorMessage == "Select the overall outcome of this application"));
        }
    }
}
