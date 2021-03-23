using System.Linq;
using FluentValidation;
using NUnit.Framework;
using SFA.DAS.ApplyService.Types;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class AppealOutcomePostRequestValidatorTests
    {
        private OutcomePostRequestValidator _validator;
        private OutcomePostRequest _request;

        [SetUp]
        public void SetUp()
        {
            _validator = new OutcomePostRequestValidator();

            _request = new OutcomePostRequest
            {
                SelectedOption = OutcomePostRequest.SubmitOption.SubmitAppealOutcome,
                SelectedAppealStatus = AppealStatus.Successful,
                SuccessfulText = "test",
                SuccessfulExternalText = "test"
            };
        }

        [Test]
        public void OversightOutcomeValidator_returns_success_when_request_is_valid()
        {
            var result = _validator.Validate(_request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.AppealOutcome));
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void OversightOutcomeValidator_appeal_returns_error_when_appealstatus_is_empty()
        {
            _request.SelectedAppealStatus = AppealStatus.None;
            var result = _validator.Validate(_request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.AppealOutcome));
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(x => x.PropertyName == nameof(OutcomePostRequest.SelectedAppealStatus)));
        }

        [TestCase(AppealStatus.Successful, "test", "test", false)]
        [TestCase(AppealStatus.Successful, "", "test", true)]
        [TestCase(AppealStatus.Successful, "test", "", true)]
        [TestCase(AppealStatus.SuccessfulAlreadyActive, "test", "test", false)]
        [TestCase(AppealStatus.SuccessfulAlreadyActive, "", "test", true)]
        [TestCase(AppealStatus.SuccessfulAlreadyActive, "test", "", true)]
        [TestCase(AppealStatus.UnsuccessfulPartiallyUpheld, "test", "test", false)]
        [TestCase(AppealStatus.UnsuccessfulPartiallyUpheld, "", "test", true)]
        [TestCase(AppealStatus.UnsuccessfulPartiallyUpheld, "test", "", true)]
        [TestCase(AppealStatus.Unsuccessful, "test", "test", false)]
        [TestCase(AppealStatus.Unsuccessful, "", "test", true)]
        [TestCase(AppealStatus.Unsuccessful, "test", "", true)]
        public void OversightOutcomeValidator_appeal_returns_errors_when_comments_are_empty(AppealStatus appealStatus, string internalComments, string externalComments, bool expectError)
        {
            SetRequestAppealStatusAndComments(appealStatus, internalComments, externalComments);

            var result = _validator.Validate(_request, options => options.IncludeRuleSets(OutcomePostRequestValidator.RuleSets.AppealOutcome));
            Assert.AreEqual(!expectError, result.IsValid);
        }

        private void SetRequestAppealStatusAndComments(AppealStatus appealStatus,
            string internalComments, string externalComments)
        {
            _request.SelectedAppealStatus = appealStatus;

            switch (appealStatus)
            {
                case AppealStatus.Successful:
                    _request.SuccessfulText = internalComments;
                    _request.SuccessfulExternalText = externalComments;
                    break;
                case AppealStatus.SuccessfulAlreadyActive:
                    _request.SuccessfulAlreadyActiveText = internalComments;
                    _request.SuccessfulAlreadyActiveExternalText = externalComments;
                    break;
                case AppealStatus.Unsuccessful:
                    _request.UnsuccessfulText = internalComments;
                    _request.UnsuccessfulExternalText = externalComments;
                    break;
                case AppealStatus.UnsuccessfulPartiallyUpheld:
                    _request.UnsuccessfulPartiallyUpheldText = internalComments;
                    _request.UnsuccessfulPartiallyUpheldExternalText = externalComments;
                    break;
            }
        }
    }
}
