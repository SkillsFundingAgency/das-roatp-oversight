﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.Interfaces;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

namespace SFA.DAS.RoatpOversight.Web.Services
{
    public class ApplicationOutcomeOrchestrator : IApplicationOutcomeOrchestrator
    {
        private readonly IApplyApiClient _applicationApiClient;
        private readonly IRoatpRegisterApiClient _registerApiClient;
        private readonly IRoatpOversightApiClient _roatpV2ApiClient;
        private readonly ILogger<ApplicationOutcomeOrchestrator> _logger;

        public ApplicationOutcomeOrchestrator(IApplyApiClient applicationApiClient, IRoatpRegisterApiClient registerApiClient,
            IRoatpOversightApiClient roatpV2ApiClient, ILogger<ApplicationOutcomeOrchestrator> logger)
        {
            _applicationApiClient = applicationApiClient;
            _registerApiClient = registerApiClient;
            _roatpV2ApiClient = roatpV2ApiClient;
            _logger = logger;
        }

        public async Task<bool> RecordOutcome(Guid applicationId, bool? approveGateway, bool? approveModeration, OversightReviewStatus outcome, string userId, string userName, string internalComments, string externalComments)
        {
            _logger.LogInformation($"Recording an oversight outcome of {outcome} for application {applicationId}");

            var registrationDetails = await _applicationApiClient.GetRegistrationDetails(applicationId);
            var registerStatus = await _registerApiClient.GetOrganisationRegisterStatus(new GetOrganisationRegisterStatusRequest { UKPRN = registrationDetails.UKPRN });

            ValidateStatusAgainstExistingStatus(outcome, registerStatus, registrationDetails.UKPRN);

            var updateOutcomeCommand = new RecordOversightOutcomeCommand
            {
                ApplicationId = applicationId,
                ApproveGateway = approveGateway,
                ApproveModeration = approveModeration,
                OversightStatus = outcome,
                UserId = userId,
                UserName = userName,
                InternalComments = internalComments,
                ExternalComments = externalComments
            };

            var updateOutcomeSuccess = await _applicationApiClient.RecordOutcome(updateOutcomeCommand);

            if (!updateOutcomeSuccess) return false;

            if (outcome == OversightReviewStatus.Successful)
            {
                var request = BuildCreateOrganisationRequest(userName, registrationDetails);


                var createOrganisationResponse = await _registerApiClient.CreateOrganisation(request);
                if (createOrganisationResponse)
                {
                    await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName,
                        request.ProviderTypeId);
                }
            }

            if ((outcome == OversightReviewStatus.SuccessfulAlreadyActive ||
                 outcome == OversightReviewStatus.SuccessfulFitnessForFunding) && registerStatus.OrganisationId != null)
            {
                await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, registrationDetails.ProviderTypeId);

                var updateOrganisationRequest = new UpdateOrganisationRequest
                {
                    ApplicationDeterminedDate = DateTime.UtcNow.Date,
                    LegalName = registrationDetails.LegalName,
                    OrganisationId = registerStatus.OrganisationId.Value,
                    Username = userName,
                    CharityNumber = registrationDetails.CharityNumber,
                    CompanyNumber = registrationDetails.CompanyNumber,
                    OrganisationTypeId = registrationDetails.OrganisationTypeId,
                    ProviderTypeId = registrationDetails.ProviderTypeId,
                    TradingName = registrationDetails.TradingName,
                };

                _logger.LogInformation($"Updating organisation details for application {applicationId}");

                return await _registerApiClient.UpdateOrganisation(updateOrganisationRequest);
            }

            return true;
        }

        public async Task<bool> RecordAppeal(Guid applicationId, string appealStatus, string userId, string userName, string internalComments,
            string externalComments)
        {
            _logger.LogInformation($"Recording an appeal outcome of {appealStatus} for application {applicationId}");
            var registrationDetails = await _applicationApiClient.GetRegistrationDetails(applicationId);
            var registerStatus = await _registerApiClient.GetOrganisationRegisterStatus(new GetOrganisationRegisterStatusRequest { UKPRN = registrationDetails.UKPRN });

            ValidateAppealStatusAgainstExistingStatus(appealStatus, registerStatus, registrationDetails.UKPRN);

            var updateAppealCommand = new RecordAppealOutcomeCommand
            {
                ApplicationId = applicationId,
                AppealStatus = appealStatus,
                UserId = userId,
                UserName = userName,
                InternalComments = internalComments,
                ExternalComments = externalComments
            };

            var updateOutcomeSuccess = await _applicationApiClient.RecordAppeal(updateAppealCommand);

            if (!updateOutcomeSuccess) return false;

            return await RecordInRoatp(applicationId, appealStatus, userId, userName, registerStatus, registrationDetails);
        }

        public async Task RecordGatewayFailOutcome(Guid applicationId, string userId, string userName)
        {
            _logger.LogInformation($"Recording an oversight gateway fail outcome for application {applicationId}");

            var command = new RecordOversightGatewayFailOutcomeCommand
            {
                ApplicationId = applicationId,
                UserId = userId,
                UserName = userName
            };

            await _applicationApiClient.RecordGatewayFailOutcome(command);
        }

        public async Task RecordGatewayRemovedOutcome(Guid applicationId, string userId, string userName)
        {
            _logger.LogInformation($"Recording an oversight gateway removed outcome for application {applicationId}");

            var command = new RecordOversightGatewayRemovedOutcomeCommand
            {
                ApplicationId = applicationId,
                UserId = userId,
                UserName = userName
            };

            await _applicationApiClient.RecordGatewayRemovedOutcome(command);
        }

        private async Task<bool> RecordInRoatp(Guid applicationId, string appealStatus, string userId, string userName,
            OrganisationRegisterStatus registerStatus, RoatpRegistrationDetails registrationDetails)
        {
            var application = await _applicationApiClient.GetApplicationDetails(applicationId);

            if (application.GatewayReviewStatus != GatewayReviewStatus.Fail)
            {
                if (registerStatus?.OrganisationId != null && (appealStatus == AppealStatus.SuccessfulAlreadyActive ||
                                                               appealStatus == AppealStatus.SuccessfulFitnessForFunding))
                {
                    await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, registrationDetails.ProviderTypeId);

                    var updateOrganisationRequest = new UpdateOrganisationRequest
                    {
                        ApplicationDeterminedDate = DateTime.UtcNow.Date,
                        LegalName = registrationDetails.LegalName,
                        OrganisationId = registerStatus.OrganisationId.Value,
                        Username = userName,
                        CharityNumber = registrationDetails.CharityNumber,
                        CompanyNumber = registrationDetails.CompanyNumber,
                        OrganisationTypeId = registrationDetails.OrganisationTypeId,
                        ProviderTypeId = registrationDetails.ProviderTypeId,
                        TradingName = registrationDetails.TradingName,
                    };

                    _logger.LogInformation($"Updating organisation details for application {applicationId}");

                    return await _registerApiClient.UpdateOrganisation(updateOrganisationRequest);
                }

                if (appealStatus == AppealStatus.Successful)
                {
                    var request = BuildCreateOrganisationRequest(userName, registrationDetails);

                    var hasSuccessfullyCreatedOrganisation = await _registerApiClient.CreateOrganisation(request);
                    if (hasSuccessfullyCreatedOrganisation)
                    {
                        await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, request.ProviderTypeId);
                    }

                    return hasSuccessfullyCreatedOrganisation;
                }

            }
            return true;
        }

        private static CreateRoatpOrganisationRequest BuildCreateOrganisationRequest(string userName, RoatpRegistrationDetails registrationDetails)
        {
            return new CreateRoatpOrganisationRequest
            {
                ApplicationDeterminedDate = DateTime.UtcNow.Date,
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
                Username = userName
            };
        }

        private async Task AddProviderToRoatpCourseManagement(RoatpRegistrationDetails registrationDetails, string userId, string userName, int providerType)
        {
            if (providerType != ProviderType.Main) return;

            var providerQuest = BuildCreateProviderRequest(userName, userId, registrationDetails);

            try
            {
                await _roatpV2ApiClient.CreateProvider(providerQuest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create provider failed for ukprn {ukprn}", registrationDetails.UKPRN);
            }
        }

        private CreateRoatpV2ProviderRequest BuildCreateProviderRequest(string userName, string userId, RoatpRegistrationDetails registrationDetails)
        {
            return new CreateRoatpV2ProviderRequest
            {
                LegalName = registrationDetails.LegalName,
                TradingName = registrationDetails.TradingName,
                Ukprn = registrationDetails.UKPRN,
                UserDisplayName = userName,
                UserId = userId
            };
        }

        private void ValidateStatusAgainstExistingStatus(OversightReviewStatus outcome, OrganisationRegisterStatus registerStatus, string ukprn)
        {
            if (outcome == OversightReviewStatus.Successful)
            {
                if (registerStatus.UkprnOnRegister)
                {
                    throw new InvalidOperationException($"Unable to register successful provider {ukprn} - already on register");
                }
            }

            if (outcome == OversightReviewStatus.SuccessfulAlreadyActive || outcome == OversightReviewStatus.SuccessfulFitnessForFunding)
            {
                if (!registerStatus.UkprnOnRegister)
                {
                    throw new InvalidOperationException(
                        $"Unable to update determined date for provider {ukprn} - provider not on register");
                }
            }
        }

        private void ValidateAppealStatusAgainstExistingStatus(string appealStatus, OrganisationRegisterStatus registerStatus, string ukprn)
        {
            if (appealStatus == AppealStatus.Successful)
            {
                if (registerStatus.UkprnOnRegister)
                {
                    throw new InvalidOperationException($"Unable to set appealStatus to 'Successful' - provider {ukprn} - already on register");
                }
            }

            if (appealStatus == AppealStatus.SuccessfulAlreadyActive || appealStatus == AppealStatus.SuccessfulFitnessForFunding)
            {
                if (!registerStatus.UkprnOnRegister)
                {
                    throw new InvalidOperationException(
                        $"Unable to update appeal determined date for provider {ukprn} - provider not on register");
                }
            }
        }
    }
}
