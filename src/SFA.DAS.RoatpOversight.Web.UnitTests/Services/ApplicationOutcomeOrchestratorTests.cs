using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.Services;
using System;

namespace SFA.DAS.RoatpOversight.Web.UnitTests.Services
{
    [TestFixture]
    public class ApplicationOutcomeOrchestratorTests
    {
        private Mock<IRoatpApplicationApiClient> _applicationApiClient;
        private Mock<IRoatpRegisterApiClient> _roatpRegisterApiClient;
        private Mock<ILogger<ApplicationOutcomeOrchestrator>> _logger;
        private ApplicationOutcomeOrchestrator _orchestrator;
        private const string UserName = "test user";
        private Guid _applicationId;

        [SetUp]
        public void Before_each_test()
        {
            _applicationApiClient = new Mock<IRoatpApplicationApiClient>();
            _roatpRegisterApiClient = new Mock<IRoatpRegisterApiClient>();
            _logger = new Mock<ILogger<ApplicationOutcomeOrchestrator>>();
            _applicationId = Guid.NewGuid();
        }

        [Test]
        public void Application_status_and_register_updated_for_a_successful_oversight_review()
        {
            _applicationApiClient.Setup(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId))).ReturnsAsync(true);

            var registrationDetails = new RoatpRegistrationDetails
            {
                CharityNumber = "112233",
                CompanyNumber = "1122111",
                LegalName = "Legal Name",
                OrganisationTypeId = 2,
                ProviderTypeId = 1,
                TradingName = "Trading as name",
                UKPRN = "19902000"
            };

            _applicationApiClient.Setup(x => x.GetRegistrationDetails(_applicationId)).ReturnsAsync(registrationDetails);

            _roatpRegisterApiClient.Setup(x => x.CreateOrganisation(It.Is<CreateRoatpOrganisationRequest>(y => y.Ukprn == registrationDetails.UKPRN))).ReturnsAsync(true);

            _orchestrator = new ApplicationOutcomeOrchestrator(_applicationApiClient.Object, _roatpRegisterApiClient.Object, _logger.Object);

            var result = _orchestrator.RecordOutcome(_applicationId, OversightReviewStatus.Successful, UserName).GetAwaiter().GetResult();

            result.Should().BeTrue();

            _applicationApiClient.Verify(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId)), Times.Once);
            _applicationApiClient.Verify(x => x.GetRegistrationDetails(_applicationId), Times.Once);
            _roatpRegisterApiClient.Verify(x => x.CreateOrganisation(It.Is<CreateRoatpOrganisationRequest>(y => y.Ukprn == registrationDetails.UKPRN)), Times.Once);
        }
    

        [Test]
        public void Application_status_updated_only_for_an_unsuccessful_oversight_review()
        {
            _applicationApiClient.Setup(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId))).ReturnsAsync(true);

            _orchestrator = new ApplicationOutcomeOrchestrator(_applicationApiClient.Object, _roatpRegisterApiClient.Object, _logger.Object);

            var result = _orchestrator.RecordOutcome(_applicationId, OversightReviewStatus.Unsuccessful, UserName).GetAwaiter().GetResult();

            result.Should().BeTrue();

            _applicationApiClient.Verify(x => x.RecordOutcome(It.Is<RecordOversightOutcomeCommand>(y => y.ApplicationId == _applicationId)), Times.Once);
            _applicationApiClient.Verify(x => x.GetRegistrationDetails(_applicationId), Times.Never);
            _roatpRegisterApiClient.Verify(x => x.CreateOrganisation(It.IsAny<CreateRoatpOrganisationRequest>()), Times.Never);
        }
    }
}
