﻿using System.Linq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Validators;
using FluentValidation;

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

            var result = _validator.Validate(request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.Default));
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
                SelectedOption = OutcomePostRequest.SubmitOption.SubmitOutcome,
                ApproveGateway = GatewayReviewStatus.Pass,
                ApproveModeration = ModerationReviewStatus.Pass,
                OversightStatus = OversightReviewStatus.InProgress,
                InProgressInternalText = internalComments,
                InProgressExternalText = externalComments
            };

            var result = _validator.Validate(request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.Default));
            Assert.AreEqual(numberOfErrorsExpected, result.Errors.Count);
        }

        [TestCase(OversightReviewStatus.None)]
        public void OversightOutcomeValidator_returns_expected_error_message_when_status_is_empty(OversightReviewStatus oversightStatus)
        {
            var request = new OutcomePostRequest
            {
                SelectedOption = OutcomePostRequest.SubmitOption.SubmitOutcome,
                OversightStatus = oversightStatus,
                ApproveGateway = ApprovalStatus.Approve,
                ApproveModeration = ApprovalStatus.Approve
            };
            var result = _validator.Validate(request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.Default));

            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == "OversightStatus" && x.ErrorMessage == "Select the overall outcome of this application"));
        }

        [TestCase(true, true, true)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        public void OversightOutcomeValidator_unsuccessful_and_an_overturned_outcome_mandates_external_comments(bool overturnGateway, bool overturnModeration, bool expectError)
        {
            var request = new OutcomePostRequest
            {
                SelectedOption = OutcomePostRequest.SubmitOption.SubmitOutcome,
                OversightStatus = OversightReviewStatus.Unsuccessful,
                ApproveGateway = overturnGateway ? ApprovalStatus.Overturn : ApprovalStatus.Approve,
                ApproveModeration = overturnModeration ? ApprovalStatus.Overturn : ApprovalStatus.Approve,
                UnsuccessfulExternalText = string.Empty
            };

            var result = _validator.Validate(request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.Default));

            Assert.AreEqual(expectError, result.Errors.Any(x => x.PropertyName == nameof(OutcomePostRequest.UnsuccessfulExternalText) && x.ErrorMessage == "Enter external comments"));
        }
    }
}
