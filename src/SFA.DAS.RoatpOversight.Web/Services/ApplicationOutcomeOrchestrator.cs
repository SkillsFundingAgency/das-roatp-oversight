using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Refit;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Domain.Interfaces;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;

namespace SFA.DAS.RoatpOversight.Web.Services;

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
        Organisation organisation = organisationResponse.Content;

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

        if (outcome is OversightReviewStatus.Successful or OversightReviewStatus.SuccessfulAlreadyActive or OversightReviewStatus.SuccessfulFitnessForFunding)
        {
            return await RecordInRoatp(applicationId, userId, userName, organisation, registrationDetails);
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
        Organisation organisation = organisationResponse.Content;

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

        var application = await _applicationApiClient.GetApplicationDetails(applicationId);
        if (application.GatewayReviewStatus == GatewayReviewStatus.Fail) return true;

        if (appealStatus is AppealStatus.Successful or AppealStatus.SuccessfulAlreadyActive or AppealStatus.SuccessfulFitnessForFunding)
        {
            return await RecordInRoatp(applicationId, userId, userName, organisation, registrationDetails);
        }

        return true;
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

    private async Task<bool> RecordInRoatp(Guid applicationId, string userId, string userName, Organisation organisation, RoatpRegistrationDetails registrationDetails)
    {
        var isUkprnOnRegister = organisation != null;
        if (isUkprnOnRegister)
        {
            var updateOrganisationRequest = new UpdateOrganisationRequest
            {
                LegalName = registrationDetails.LegalName,
                RequestingUserId = userName,
                CharityNumber = registrationDetails.CharityNumber,
                CompanyNumber = registrationDetails.CompanyNumber,
                OrganisationTypeId = registrationDetails.OrganisationTypeId,
                ProviderType = (ProviderType)registrationDetails.ProviderTypeId,
                TradingName = registrationDetails.TradingName,
            };

            _logger.LogInformation("Updating organisation details for application {ApplicationId}", applicationId);

            HttpResponseMessage response = await _registerApiClient.UpdateOrganisation(int.Parse(registrationDetails.UKPRN), updateOrganisationRequest);

            if (!response.IsSuccessStatusCode) return false;
        }
        else
        {
            var request = new CreateRoatpOrganisationRequest
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

            HttpResponseMessage response = await _registerApiClient.CreateOrganisation(request);

            if (!response.IsSuccessStatusCode) return false;
        }

        await UpdateAllowedCourseTypes((ProviderType)registrationDetails.ProviderTypeId, registrationDetails.UKPRN, userName, organisation);

        await AddProviderToRoatpCourseManagement(registrationDetails, userId, userName, registrationDetails.ProviderTypeId);

        return true;
    }

    private async Task UpdateAllowedCourseTypes(ProviderType providerType, string ukprn, string userName, Organisation organisation)
    {
        if (providerType == ProviderType.Supporting) return;
        List<int> courseTypeIds = organisation?.AllowedCourseTypes.Select(ct => ct.CourseTypeId).ToList() ?? [];
        if (courseTypeIds.Contains(CourseType.Apprenticeship)) return;
        courseTypeIds.Add(CourseType.Apprenticeship);
        await _registerApiClient.UpdateCourseTypes(int.Parse(ukprn), new UpdateCourseTypesRequest(courseTypeIds, userName));
    }

    private async Task AddProviderToRoatpCourseManagement(RoatpRegistrationDetails registrationDetails, string userId, string userName, int providerType)
    {
        if (providerType != ProviderTypeConstants.Main) return;

        var providerQuest = new CreateRoatpV2ProviderRequest
        {
            LegalName = registrationDetails.LegalName,
            TradingName = registrationDetails.TradingName,
            Ukprn = registrationDetails.UKPRN,
            UserDisplayName = userName,
            UserId = userId
        };

        try
        {
            await _roatpV2ApiClient.CreateProvider(providerQuest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create provider failed for ukprn {Ukprn}", registrationDetails.UKPRN);
        }
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
