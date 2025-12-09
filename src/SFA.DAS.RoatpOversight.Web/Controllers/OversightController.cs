using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Domain;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Extensions;
using SFA.DAS.RoatpOversight.Web.Infrastructure.ApiClients;
using SFA.DAS.RoatpOversight.Web.ModelBinders;
using SFA.DAS.RoatpOversight.Web.Models;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Validators;

namespace SFA.DAS.RoatpOversight.Web.Controllers;

[Authorize(Roles = Roles.RoatpApplicationOversightTeam)]
public class OversightController : Controller
{
    private readonly ISearchTermValidator _searchTermValidator;
    private readonly IApplicationOutcomeOrchestrator _outcomeOrchestrator;
    private readonly IOversightOrchestrator _oversightOrchestrator;
    private readonly IApplyApiClient _apiClient;
    private readonly AppealPostRequestValidator _appealPostRequestValidator;
    private readonly OutcomePostRequestValidator _outcomePostRequestValidator;
    private readonly ConfirmOutcomePostRequestValidator _confirmOutcomePostRequestValidator;
    private readonly ConfirmAppealOutcomePostRequestValidator _confirmAppealOutcomePostRequestValidator;

    public OversightController(ISearchTermValidator searchTermValidator,
                               IApplicationOutcomeOrchestrator outcomeOrchestrator,
                               IOversightOrchestrator oversightOrchestrator, 
                               IApplyApiClient apiClient,
                               AppealPostRequestValidator appealPostRequestValidator,
                               OutcomePostRequestValidator outcomePostRequestValidator,
                               ConfirmOutcomePostRequestValidator confirmOutcomePostRequestValidator,
                               ConfirmAppealOutcomePostRequestValidator confirmAppealOutcomePostRequestValidator)
    {
        _searchTermValidator = searchTermValidator;
        _outcomeOrchestrator = outcomeOrchestrator;
        _oversightOrchestrator = oversightOrchestrator;
        _apiClient = apiClient;
        _appealPostRequestValidator = appealPostRequestValidator;
        _outcomePostRequestValidator = outcomePostRequestValidator;
        _confirmOutcomePostRequestValidator = confirmOutcomePostRequestValidator;
        _confirmAppealOutcomePostRequestValidator = confirmAppealOutcomePostRequestValidator;
    }

    public async Task<IActionResult> Applications(string selectedTab, [StringTrim] string searchTerm, string sortColumn, string sortOrder)
    {
        if (searchTerm != null)
        {
            var validationResponse = _searchTermValidator.Validate(searchTerm);

            foreach (var error in validationResponse.Errors)
            {
                ModelState.AddModelError(error.Field, error.ErrorMessage);
            }
        }

        var viewModel = await _oversightOrchestrator.GetApplicationsViewModel(selectedTab, ModelState.IsValid ? searchTerm : null, sortColumn, sortOrder);
        return View(viewModel);
    }

    [HttpGet("Oversight/Outcome/{applicationId}")]
    public async Task<IActionResult> Outcome(OutcomeRequest request)
    {
        try
        {
            var vm = await _oversightOrchestrator.GetOversightDetailsViewModel(request.ApplicationId, request.OutcomeKey);
            return View(vm);
        }
        catch (InvalidStateException)
        {
            return RedirectToAction(nameof(Applications));
        }
    }


    [HttpPost("Oversight/Outcome/{applicationId}")]
    public async Task<IActionResult> Outcome(OutcomePostRequest request)
    {
        if (request.SelectedOption == OutcomePostRequest.SubmitOption.SubmitOutcome)
        {
            var validationResult = _outcomePostRequestValidator.Validate(request);
            if (!validationResult.IsValid)
            {
                ModelState.AddValidationErrors(validationResult.Errors);
                var vm = await _oversightOrchestrator.GetOversightDetailsViewModel(request.ApplicationId, request.OutcomeKey);
                return View(vm);
            }
        }

        var userId = HttpContext.User.UserId();
        var userName = HttpContext.User.UserDisplayName();

        if (request.IsGatewayRemoved)
        {
            await _outcomeOrchestrator.RecordGatewayRemovedOutcome(request.ApplicationId, userId, userName);
            return RedirectToAction("Confirmed", new { request.ApplicationId });
        }

        if (request.IsGatewayFail)
        {
            await _outcomeOrchestrator.RecordGatewayFailOutcome(request.ApplicationId, userId, userName);
            return RedirectToAction("Confirmed", new { request.ApplicationId });
        }

        var cacheKey = await _oversightOrchestrator.SaveOutcomePostRequestToCache(request);
        return RedirectToAction("ConfirmOutcome", new { applicationId = request.ApplicationId, OutcomeKey = cacheKey });
    }

    [HttpGet("Oversight/Appeal/{applicationId}")]
    public async Task<IActionResult> Appeal(AppealRequest request)
    {
        try
        {
            var vm = await _oversightOrchestrator.GetAppealDetailsViewModel(request.ApplicationId, request.OutcomeKey);
            return View(vm);
        }
        catch (InvalidStateException)
        {
            return RedirectToAction(nameof(Applications));
        }
    }


    [HttpPost("Oversight/Appeal/{applicationId}")]
    public async Task<IActionResult> Appeal(AppealPostRequest request)
    {
        var validationResult = _appealPostRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            var vm = await _oversightOrchestrator.GetAppealDetailsViewModel(request.ApplicationId, request.OutcomeKey);
            return View(vm);
        }

        var cacheKey = await _oversightOrchestrator.SaveAppealPostRequestToCache(request);
        return RedirectToAction("ConfirmAppealOutcome", new { applicationId = request.ApplicationId, OutcomeKey = cacheKey });
    }

    [HttpGet("Oversight/AppealOutcome/{applicationId}")]
    public async Task<IActionResult> AppealOutcome(AppealRequest request)
    {
        try
        {
            var vm = await _oversightOrchestrator.GetAppealDetailsViewModel(request.ApplicationId, request.OutcomeKey);
            return View(vm);
        }
        catch (InvalidStateException)
        {
            return RedirectToAction("Applications");
        }
    }

    [HttpGet("Oversight/Appeal/{applicationId}/confirm/{outcomeKey}")]
    public async Task<IActionResult> ConfirmAppealOutcome(ConfirmAppealOutcomeRequest request)
    {
        try
        {
            var viewModel = await _oversightOrchestrator.GetConfirmAppealOutcomeViewModel(request.ApplicationId, request.OutcomeKey);
            return View(viewModel);
        }
        catch (ConfirmOutcomeCacheKeyNotFoundException)
        {
            return RedirectToAction("Outcome", new { request.ApplicationId });
        }
        catch (InvalidStateException)
        {
            return RedirectToAction("Applications");
        }
    }


    [HttpPost("Oversight/Appeal/{applicationId}/confirm/{outcomeKey}")]
    public async Task<IActionResult> ConfirmAppealOutcome(ConfirmAppealOutcomePostRequest request)
    {
        var validationResult = _confirmAppealOutcomePostRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            var viewModel = await _oversightOrchestrator.GetConfirmAppealOutcomeViewModel(request.ApplicationId, request.OutcomeKey);
            return View(viewModel);
        }
        if (request.Confirm == OversightConfirmationStatus.No)
        {
            return RedirectToAction("Appeal", new { request.ApplicationId, request.OutcomeKey });
        }

        var userId = HttpContext.User.UserId();
        var userName = HttpContext.User.UserDisplayName();

        await _outcomeOrchestrator.RecordAppeal(request.ApplicationId, request.AppealStatus, userId, userName, request.InternalComments, request.ExternalComments);

        return RedirectToAction("AppealConfirmed", new { request.ApplicationId });
    }

    [HttpGet("Oversight/Appeal/{applicationId}/confirmed")]
    public async Task<IActionResult> AppealConfirmed(AppealConfirmedRequest request)
    {
        var viewModel = await _oversightOrchestrator.GetAppealConfirmedViewModel(request.ApplicationId);

        return View(viewModel);
    }

    [HttpGet("Oversight/Outcome/{applicationId}/confirm/{outcomeKey}")]
    public async Task<IActionResult> ConfirmOutcome([FromRoute] Guid applicationId, [FromRoute] Guid outcomeKey)
    {
        try
        {
            var viewModel = await _oversightOrchestrator.GetConfirmOutcomeViewModel(applicationId, outcomeKey);
            return View(viewModel);
        }
        catch (ConfirmOutcomeCacheKeyNotFoundException)
        {
            return RedirectToAction("Outcome", new { applicationId });
        }
        catch (InvalidStateException)
        {
            return RedirectToAction("Applications");
        }
    }

    [HttpPost("Oversight/Outcome/{applicationId}/confirm/{outcomeKey}")]
    public async Task<IActionResult> ConfirmOutcome([FromRoute] Guid applicationId, [FromRoute] Guid outcomeKey, ConfirmOutcomePostRequest request)
    {
        var validationResult = _confirmOutcomePostRequestValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            ModelState.AddValidationErrors(validationResult.Errors);
            var viewModel = await _oversightOrchestrator.GetConfirmOutcomeViewModel(applicationId, outcomeKey);
            return View(viewModel);
        }

        if (request.Confirm == OversightConfirmationStatus.No)
        {
            return RedirectToAction("Outcome", new { request.ApplicationId, request.OutcomeKey });
        }

        var userId = HttpContext.User.UserId();
        var userName = HttpContext.User.UserDisplayName();

        var approveGateway = string.IsNullOrEmpty(request.ApproveGateway) ? default(bool?) : request.ApproveGateway == ApprovalStatus.Approve;
        var approveModeration = string.IsNullOrEmpty(request.ApproveModeration) ? default(bool?) : request.ApproveModeration == ApprovalStatus.Approve;

        await _outcomeOrchestrator.RecordOutcome(request.ApplicationId, approveGateway, approveModeration, request.OversightStatus, userId, userName, request.InternalComments, request.ExternalComments);

        return RedirectToAction("Confirmed", new { request.ApplicationId });
    }

    [HttpGet("Oversight/Outcome/{applicationId}/confirmed")]
    public async Task<IActionResult> Confirmed(ConfirmedRequest request)
    {
        var viewModel = await _oversightOrchestrator.GetConfirmedViewModel(request.ApplicationId);
        return View(viewModel);
    }


    [HttpGet("Oversight/{applicationId}/appeal/file/{fileName}")]
    public async Task<IActionResult> DownloadAppealFile(Guid applicationId, string fileName)
    {
        var response = await _apiClient.DownloadFile(applicationId, fileName);

        if (response.IsSuccessStatusCode)
        {
            var fileStream = await response.Content.ReadAsStreamAsync();

            return File(fileStream, response.Content.Headers.ContentType.MediaType, response.Content.Headers.ContentDisposition.FileNameStar);
        }

        return NotFound();
    }
}
