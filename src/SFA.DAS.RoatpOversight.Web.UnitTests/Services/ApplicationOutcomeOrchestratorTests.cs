using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Services;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Services
{
    [TestFixture]
    public class ApplicationOutcomeOrchestratorTests
    {
        private Mock<IApplyApiClient> _applicationApiClient;
        private Mock<IRoatpRegisterApiClient> _roatpRegisterApiClient;
        private Mock<ILogger<ApplicationOutcomeOrchestrator>> _logger;
        private ApplicationOutcomeOrchestrator _orchestrator;
        private const string UserName = "test user";
        private const string UserId = "testUser";
        private const string InternalComments = "testinternalcomments";
        private const string ExternalComments = "testexternalcomments";
        private Guid _applicationId;
        private RoatpRegistrationDetails _registrationDetails;
        private OrganisationRegisterStatus _registerStatus;

        [SetUp]
        public void Before_each_test()
        {
            _applicationApiClient = new Mock<IApplyApiClient>();
            _roatpRegisterApiClient = new Mock<IRoatpRegisterApiClient>();
            _logger = new Mock<ILogger<ApplicationOutcomeOrchestrator>>();
            _applicationId = Guid.NewGuid();

            _registrationDetails = new RoatpRegistrationDetails
            {
                CharityNumber = "112233",
                CompanyNumber = "1122111",
                LegalName = "Legal Name",
                OrganisationTypeId = 2,
                ProviderTypeId = 1,
                TradingName = "Trading as name",
                UKPRN = "19902000"
            };

            _registerStatus = new OrganisationRegisterStatus
            {
                OrganisationId = Guid.NewGuid(),
                UkprnOnRegister = true
            };

            _applicationApiClient.Setup(x => x.GetRegistrationDetails(_applicationId)).ReturnsAsync(_registrationDetails);

            _applicationApiClient.Setup(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId))).ReturnsAsync(true);

            _roatpRegisterApiClient.Setup(x => x.CreateOrganisation(It.Is<CreateRoatpOrganisationRequest>(y => y.Ukprn == _registrationDetails.UKPRN))).ReturnsAsync(() => true);

            _roatpRegisterApiClient.Setup(x =>
                    x.GetOrganisationRegisterStatus(
                        It.Is<GetOrganisationRegisterStatusRequest>(r => r.UKPRN == _registrationDetails.UKPRN)))
                .ReturnsAsync(() => _registerStatus);

            _orchestrator = new ApplicationOutcomeOrchestrator(_applicationApiClient.Object, _roatpRegisterApiClient.Object, _logger.Object);
        }

        [Test]
        public async Task Application_status_and_register_updated_for_a_successful_oversight_review()
        {
            _registerStatus.UkprnOnRegister = false;

            var result = await _orchestrator.RecordOutcome(_applicationId, OversightReviewStatus.Successful, UserId, UserName, InternalComments, ExternalComments);

            result.Should().BeTrue();

            _applicationApiClient.Verify(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId)), Times.Once);
            _applicationApiClient.Verify(x => x.GetRegistrationDetails(_applicationId), Times.Once);
            _roatpRegisterApiClient.Verify(x => x.CreateOrganisation(It.Is<CreateRoatpOrganisationRequest>(y => y.Ukprn == _registrationDetails.UKPRN)), Times.Once);
        }

        [Test]
        public async Task Application_status_updated_only_for_an_unsuccessful_oversight_review()
        {
            var result = await _orchestrator.RecordOutcome(_applicationId, OversightReviewStatus.Unsuccessful, UserId, UserName, InternalComments, ExternalComments);

            result.Should().BeTrue();

            _applicationApiClient.Verify(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId)), Times.Once);
            _roatpRegisterApiClient.Verify(x => x.CreateOrganisation(It.IsAny<CreateRoatpOrganisationRequest>()), Times.Never);
        }

        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding)]
        public async Task Application_status_and_register_updated_for_a_successful_already_active_or_fitness_for_funding_oversight_review(string status)
        {
            await _orchestrator.RecordOutcome(_applicationId, status, UserId, UserName, InternalComments, ExternalComments);

            _applicationApiClient.Verify(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId)), Times.Once);
            _roatpRegisterApiClient.Verify(x => x.UpdateApplicationDeterminedDate(It.Is<UpdateOrganisationApplicationDeterminedDateRequest>(y => y.OrganisationId == _registerStatus.OrganisationId)), Times.Once);
        }

        [Test]
        public void Successful_oversight_review_for_provider_already_on_register_throws_exception()
        {
            _registerStatus.UkprnOnRegister = true;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _orchestrator.RecordOutcome(_applicationId, OversightReviewStatus.Successful, UserId, UserName, InternalComments, ExternalComments));
        }

        [TestCase(OversightReviewStatus.SuccessfulAlreadyActive)]
        [TestCase(OversightReviewStatus.SuccessfulFitnessForFunding)]
        public void Successful_already_active_or_fitness_for_funding_oversight_review_for_provider_not_already_on_register_throws_exception(string status)
        {
            _registerStatus.UkprnOnRegister = false;
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _orchestrator.RecordOutcome(_applicationId, status, UserId, UserName, InternalComments, ExternalComments));
        }
    }
}
