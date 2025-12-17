using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Refit;
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
        private readonly IRoatpOversightOuterApiClient _roatpV2ApiClient;
        private readonly ILogger<ApplicationOutcomeOrchestrator> _logger;

        public ApplicationOutcomeOrchestrator(IApplyApiClient applicationApiClient, IRoatpRegisterApiClient registerApiClient,
            IRoatpOversightOuterApiClient roatpV2ApiClient, ILogger<ApplicationOutcomeOrchestrator> logger)
        {
            _applicationApiClient = applicationApiClient;
            _registerApiClient = registerApiClient;
            _roatpV2ApiClient = roatpV2ApiClient;
            _logger = logger;
        }

        public async Task<bool> RecordOutcome(Guid applicationId, bool? approveGateway, bool? approveModerator, OversightReviewStatus outcome, string userId, string userName, string internalComments, string externalComments)
        {
            _logger.LogInformation("Recording an oversight outcome of {Outcome} for application {ApplicationId}", outcome, applicationId);

            var registrationDetails = await _applicationApiClient.GetRegistrationDetails(applicationId);
            ApiResponse<Organisation> organisationResponse = await _registerApiClient.GetOrganisation(int.Parse(registrationDetails.UKPRN));

            bool isUkprnOnRegister = organisationResponse.IsSuccessStatusCode;

            ValidateStatusAgainstExistingStatus(outcome, isUkprnOnRegister, registrationDetails.UKPRN);

            var updateOutcomeCommand = new RecordOversightOutcomeCommand
            {
                ApplicationId = applicationId,
                ApproveGateway = approveGateway,
                ApproveModeration = approveModerator,
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
                        (int)request.ProviderType);
                }
            }

            if (isUkprnOnRegister && (outcome is OversightReviewStatus.SuccessfulAlreadyActive or OversightReviewStatus.SuccessfulFitnessForFunding))
            {
                await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, registrationDetails.ProviderTypeId);

                var updateOrganisationRequest = new UpdateOrganisationRequest
                {
                    LegalName = registrationDetails.LegalName,
                    RequestingUserId = userName,
                    CharityNumber = registrationDetails.CharityNumber,
                    CompanyNumber = registrationDetails.CompanyNumber,
                    OrganisationTypeId = registrationDetails.OrganisationTypeId,
                    ProviderTypeId = registrationDetails.ProviderTypeId,
                    TradingName = registrationDetails.TradingName,
                };

                _logger.LogInformation("Updating organisation details for application {ApplicationId}", applicationId);

                return await _registerApiClient.UpdateOrganisation(int.Parse(registrationDetails.UKPRN), updateOrganisationRequest);
            }

            return true;
        }

        public async Task<bool> RecordAppeal(Guid applicationId, string appealStatus, string userId, string userName, string internalComments,
            string externalComments)
        {
            _logger.LogInformation("Recording an appeal outcome of {AppealStatus} for application {ApplicationId}", appealStatus, applicationId);
            var registrationDetails = await _applicationApiClient.GetRegistrationDetails(applicationId);
            ApiResponse<Organisation> organisationResponse = await _registerApiClient.GetOrganisation(int.Parse(registrationDetails.UKPRN));
            bool isUkprnOnRegister = organisationResponse.IsSuccessStatusCode;

            ValidateAppealStatusAgainstExistingStatus(appealStatus, isUkprnOnRegister, registrationDetails.UKPRN);

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

            return await RecordInRoatp(applicationId, appealStatus, userId, userName, isUkprnOnRegister, registrationDetails);
        }

        public async Task RecordGatewayFailOutcome(Guid applicationId, string userId, string userName)
        {
            _logger.LogInformation("Recording an oversight gateway fail outcome for application {ApplicationId}", applicationId);

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
            _logger.LogInformation("Recording an oversight gateway removed outcome for application {ApplicationId}", applicationId);

            var command = new RecordOversightGatewayRemovedOutcomeCommand
            {
                ApplicationId = applicationId,
                UserId = userId,
                UserName = userName
            };

            await _applicationApiClient.RecordGatewayRemovedOutcome(command);
        }

        private async Task<bool> RecordInRoatp(Guid applicationId, string appealStatus, string userId, string userName,
            bool isUkprnOnRegister, RoatpRegistrationDetails registrationDetails)
        {
            var application = await _applicationApiClient.GetApplicationDetails(applicationId);

            if (application.GatewayReviewStatus != GatewayReviewStatus.Fail)
            {
                if (isUkprnOnRegister && (appealStatus is AppealStatus.SuccessfulAlreadyActive or AppealStatus.SuccessfulFitnessForFunding))
                {
                    await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, registrationDetails.ProviderTypeId);

                    var updateOrganisationRequest = new UpdateOrganisationRequest
                    {
                        LegalName = registrationDetails.LegalName,
                        RequestingUserId = userName,
                        CharityNumber = registrationDetails.CharityNumber,
                        CompanyNumber = registrationDetails.CompanyNumber,
                        OrganisationTypeId = registrationDetails.OrganisationTypeId,
                        ProviderTypeId = registrationDetails.ProviderTypeId,
                        TradingName = registrationDetails.TradingName,
                    };

                    _logger.LogInformation("Updating organisation details for application {ApplicationId}", applicationId);

                    return await _registerApiClient.UpdateOrganisation(int.Parse(registrationDetails.UKPRN), updateOrganisationRequest);
                }

                if (appealStatus == AppealStatus.Successful)
                {
                    var request = BuildCreateOrganisationRequest(userName, registrationDetails);

                    var hasSuccessfullyCreatedOrganisation = await _registerApiClient.CreateOrganisation(request);
                    if (hasSuccessfullyCreatedOrganisation)
                    {
                        await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, (int)request.ProviderType);
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
                CharityNumber = registrationDetails.CharityNumber,
                CompanyNumber = registrationDetails.CompanyNumber,
                LegalName = registrationDetails.LegalName,
                OrganisationTypeId = registrationDetails.OrganisationTypeId,
                ProviderType = (ProviderType)registrationDetails.ProviderTypeId,
                TradingName = registrationDetails.TradingName,
                Ukprn = registrationDetails.UKPRN,
                RequestingUserId = userName
            };
        }

        private async Task AddProviderToRoatpCourseManagement(RoatpRegistrationDetails registrationDetails, string userId, string userName, int providerType)
        {
            if (providerType != ProviderTypeConstants.Main) return;

            var providerQuest = BuildCreateProviderRequest(userName, userId, registrationDetails);

            try
            {
                await _roatpV2ApiClient.CreateProvider(providerQuest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create provider failed for ukprn {Ukprn}", registrationDetails.UKPRN);
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

        private static void ValidateStatusAgainstExistingStatus(OversightReviewStatus outcome, bool isUkprnOnRegister, string ukprn)
        {
            if (outcome == OversightReviewStatus.Successful)
            {
                if (isUkprnOnRegister)
                {
                    throw new InvalidOperationException($"Unable to register successful provider {ukprn} - already on register");
                }
            }

            if (outcome == OversightReviewStatus.SuccessfulAlreadyActive || outcome == OversightReviewStatus.SuccessfulFitnessForFunding)
            {
                if (!isUkprnOnRegister)
                {
                    throw new InvalidOperationException(
                        $"Unable to update determined date for provider {ukprn} - provider not on register");
                }
            }
        }

        private static void ValidateAppealStatusAgainstExistingStatus(string appealStatus, bool isUkprnOnRegister, string ukprn)
        {
            if (appealStatus == AppealStatus.Successful)
            {
                if (isUkprnOnRegister)
                {
                    throw new InvalidOperationException($"Unable to set appealStatus to 'Successful' - provider {ukprn} - already on register");
                }
            }

            if (appealStatus == AppealStatus.SuccessfulAlreadyActive || appealStatus == AppealStatus.SuccessfulFitnessForFunding)
            {
                if (!isUkprnOnRegister)
                {
                    throw new InvalidOperationException(
                        $"Unable to update appeal determined date for provider {ukprn} - provider not on register");
                }
            }
        }
    }
}
