using System;
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
                var request = BuildCreateOrganisationRequest(updateOutcomeCommand, registrationDetails);

                var updateRegisterResult = await _registerApiClient.CreateOrganisation(request);

                return updateRegisterResult;
            }

            if (outcome == OversightReviewStatus.SuccessfulAlreadyActive ||
                outcome == OversightReviewStatus.SuccessfulFitnessForFunding)
            {
                var updateDeterminedDateRequest = new UpdateOrganisationApplicationDeterminedDateRequest
                {
                    ApplicationDeterminedDate = DateTime.UtcNow.Date,
                    LegalName = registrationDetails.LegalName,
                    OrganisationId = registerStatus.OrganisationId.Value,
                    UpdatedBy = userId
                };

                await _registerApiClient.UpdateApplicationDeterminedDate(updateDeterminedDateRequest);
            }

            return updateOutcomeSuccess;
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

            if (registerStatus?.OrganisationId != null && (appealStatus == AppealStatus.Successful ||
                                                             appealStatus == AppealStatus.SuccessfulAlreadyActive ||
                                                             appealStatus == AppealStatus.SuccessfulFitnessForFunding))
                {
                    var updateDeterminedDateRequest = new UpdateOrganisationApplicationDeterminedDateRequest
                    {
                        ApplicationDeterminedDate = DateTime.UtcNow.Date,
                        LegalName = registrationDetails.LegalName,
                        OrganisationId = registerStatus.OrganisationId.Value,
                        UpdatedBy = userId
                    };

                    await _registerApiClient.UpdateApplicationDeterminedDate(updateDeterminedDateRequest);
                }
            
            return true;
        }

        public async Task RecordGatewayFailOutcome(Guid applicationId, string userId, string userName)
        {
            _logger.LogInformation($"Recording an oversight gateway fail outcome for application {applicationId}");

            var command = new RecordOversightGatewayFailOutcomeCommand
            {
                ApplicationId = applicationId,
                UserId =  userId,
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

        private static CreateRoatpOrganisationRequest BuildCreateOrganisationRequest(RecordOversightOutcomeCommand updateOutcomeCommand, RoatpRegistrationDetails registrationDetails)
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
                Username = updateOutcomeCommand.UserName
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
