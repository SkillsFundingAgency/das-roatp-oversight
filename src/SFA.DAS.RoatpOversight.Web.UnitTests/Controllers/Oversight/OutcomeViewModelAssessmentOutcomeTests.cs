using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Models.Partials;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Controllers.Oversight
{
    [TestFixture]
    public class OutcomeViewModelAssessmentOutcomeTests
    {
        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Passed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]

        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Pass, AssessmentOutcomeStatus.Failed)]

        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Pass, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]

        [TestCase(FinancialReviewStatus.Exempt, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Exempt, null, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Pass, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Exempt, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Outstanding, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Good, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Satisfactory, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]
        [TestCase(FinancialReviewStatus.Fail, FinancialApplicationSelectedGrade.Inadequate, GatewayReviewStatus.Fail, ModerationReviewStatus.Fail, AssessmentOutcomeStatus.Failed)]

        public void OutcomeViewModel_assessment_outcome_tests(string financialReviewStatus, string financialGradeAwarded, string gatewayReviewStatus, string moderationReviewStatus, string assessmentOutcome)
        {
            var viewModel = new OutcomeViewModel
            {
                FinancialHealthOutcome = new FinancialHealthOutcomeViewModel
                {
                    FinancialReviewStatus = financialReviewStatus,
                    FinancialGradeAwarded = financialGradeAwarded
                },
                GatewayOutcome = new GatewayOutcomeViewModel
                {
                    GatewayReviewStatus = gatewayReviewStatus
                },
                ModerationOutcome = new ModerationOutcomeViewModel
                {
                    ModerationReviewStatus = moderationReviewStatus
                }
            };
            
            Assert.AreEqual(assessmentOutcome,viewModel.AssessmentOutcome);
        }
    }
}
