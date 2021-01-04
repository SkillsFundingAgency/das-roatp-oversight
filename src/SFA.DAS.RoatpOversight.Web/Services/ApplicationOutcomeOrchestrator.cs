﻿using System;
using System.Threading.Tasks;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public class ApplicationOutcomeOrchestrator : IApplicationOutcomeOrchestrator
    {
        private readonly IApplyApiClient _applicationApiClient;
        private readonly IRoatpRegisterApiClient _registerApiClient;
        private readonly ILogger<ApplicationOutcomeOrchestrator> _logger;

        public ApplicationOutcomeOrchestrator(IApplyApiClient applicationApiClient, IRoatpRegisterApiClient registerApiClient,
                                              ILogger<ApplicationOutcomeOrchestrator> logger)
        {
            _applicationApiClient = applicationApiClient;
            _registerApiClient = registerApiClient;
            _logger = logger;
        }

        public async Task<bool> RecordOutcome(Guid applicationId, string outcome, string userId, string userName)
        {
            _logger.LogInformation($"Recording an oversight outcome of {outcome} for application {applicationId}");

            var updateOutcomeCommand = new RecordOversightOutcomeCommand
            {
                ApplicationId = applicationId,
                OversightStatus = outcome,
                UserId = userId,
                UserName = userName
            };

            var updateOutcomeSuccess = await _applicationApiClient.RecordOutcome(updateOutcomeCommand);

            if (updateOutcomeSuccess && outcome == OversightReviewStatus.Successful)
            {
                var registrationDetails = await _applicationApiClient.GetRegistrationDetails(applicationId);

                var request = BuildCreateOrganisationRequest(updateOutcomeCommand, registrationDetails);

                var updateRegisterResult = await _registerApiClient.CreateOrganisation(request);

                return updateRegisterResult;
            }

            return updateOutcomeSuccess;
        }

        private static CreateRoatpOrganisationRequest BuildCreateOrganisationRequest(RecordOversightOutcomeCommand updateOutcomeCommand, RoatpRegistrationDetails registrationDetails)
        {
            return new CreateRoatpOrganisationRequest
            {
                ApplicationDeterminedDate = DateTime.Now,
                CharityNumber = registrationDetails.CharityNumber,
                CompanyNumber = registrationDetails.CompanyNumber,
                FinancialTrackRecord = true,
                LegalName = registrationDetails.LegalName,
                NonLevyContract = false,
                OrganisationTypeId = registrationDetails.OrganisationTypeId,
                ParentCompanyGuarantee = false,
                ProviderTypeId = registrationDetails.ProviderTypeId,
                SourceIsUKRLP = true,
                StatusDate = DateTime.Now,
                TradingName = registrationDetails.TradingName,
                Ukprn = registrationDetails.UKPRN,
                Username = updateOutcomeCommand.UserName
            };
        }
    }
}
