﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Validators;
using SFA.DAS.RoatpOversight.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SFA.DAS.AdminService.Common.Extensions;
using SFA.DAS.RoatpOversight.Web.Exceptions;
using SFA.DAS.RoatpOversight.Web.Models;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    [Authorize]
    public class OversightController : Controller
    {
        private readonly IApplicationOutcomeOrchestrator _outcomeOrchestrator;
        private readonly IOversightOrchestrator _oversightOrchestrator;

        public OversightController(IApplicationOutcomeOrchestrator outcomeOrchestrator,
                                   IOversightOrchestrator oversightOrchestrator)
        {
            _outcomeOrchestrator = outcomeOrchestrator;
            _oversightOrchestrator = oversightOrchestrator;
        }

        public async Task<IActionResult> Applications()
        {
            var viewModel = await _oversightOrchestrator.GetOversightOverviewViewModel();
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
            catch(InvalidStateException)
            {
                return RedirectToAction("Applications");
            }
        }

        [HttpPost("Oversight/Outcome/{applicationId}")]
        public async Task<IActionResult> Outcome(OutcomePostRequest request)
        {
            var cacheKey = await _oversightOrchestrator.SaveOutcomePostRequestToCache(request);
            return RedirectToAction("ConfirmOutcome", new {applicationId = request.ApplicationId, OutcomeKey = cacheKey});
        }

        [HttpGet("Oversight/Outcome/{applicationId}/confirm/{outcomeKey}")]
        public async Task<IActionResult> ConfirmOutcome(ConfirmOutcomeRequest request)
        {
            try
            {
                var viewModel = await _oversightOrchestrator.GetConfirmOutcomeViewModel(request.ApplicationId, request.OutcomeKey);
                return View(viewModel);
            }
            catch (ConfirmOutcomeCacheKeyNotFoundException)
            {
                return RedirectToAction("Outcome", new { request.ApplicationId });
            }
        }

        [HttpPost("Oversight/Outcome/{applicationId}/confirm/{outcomeKey}")]
        public async Task<IActionResult> ConfirmOutcome(ConfirmOutcomePostRequest request)
        {
            if (request.Confirm == OversightConfirmationStatus.No)
            {
                return RedirectToAction("Outcome", new { request.ApplicationId, request.OutcomeKey });
            }

            //todo: required state changes here

            var userId = HttpContext.User.UserId();
            var userName = HttpContext.User.UserDisplayName();

            if (request.OversightStatus == OversightReviewStatus.Successful ||
                request.OversightStatus == OversightReviewStatus.Unsuccessful)
            {
                await _outcomeOrchestrator.RecordOutcome(request.ApplicationId, request.OversightStatus, userId, userName);
            }
            else if (request.OversightStatus == OversightReviewStatus.SuccessfulAlreadyActive)
            {
                //todo
            }
            else if (request.OversightStatus == OversightReviewStatus.SuccessfulFitnessForFunding)
            {
                //todo
            }

            return RedirectToAction("Confirmed", new {request.ApplicationId});
        }

        [HttpGet("Oversight/Outcome/{applicationId}/confirmed")]
        public async Task<IActionResult> Confirmed(ConfirmedRequest request)
        {
            //todo: get all the stuff we need for vm
            //todo: rename this vm
            var viewModelDone = new OutcomeDoneViewModel { Ukprn = "TODO", Status = OversightReviewStatus.Unsuccessful };

            //todo: rename this view
            return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        }


        //[HttpPost("Oversight/Outcome/{applicationId}")]
        //public async Task<IActionResult> EvaluateOutcome(EvaluationOutcomeCommand command) 
        //{
        //    var viewModel = await _oversightOrchestrator.GetOversightDetailsViewModel(command.ApplicationId);

        //    if (CheckForBackButtonAfterSubmission(viewModel.OversightStatus, out var outcome)) return outcome;

        //    viewModel.OversightStatus = command.OversightStatus;
        //    viewModel.ApproveGateway = command.ApproveGateway;
        //    viewModel.ApproveModeration = command.ApproveModeration;
        //    viewModel.SuccessfulText = command.SuccessfulText;
        //    viewModel.SuccessfulAlreadyActiveText = command.SuccessfulAlreadyActiveText;
        //    viewModel.SuccessfulFitnessForFundingText = command.SuccessfulFitnessForFundingText;
        //    viewModel.UnsuccessfulText = command.UnsuccessfulText;
        //    viewModel.InProgressInternalText = command.InProgressInternalText;
        //    viewModel.InProgressExternalText = command.InProgressExternalText;
        //    viewModel.ApplicationEmailAddress = command.ApplicationEmailAddress;

        //    var errorMessages = OversightValidator.ValidateOverallOutcome(command);

        //    if (errorMessages.Any())
        //    {
        //        viewModel.ErrorMessages = errorMessages;
        //        return View($"~/Views/Oversight/Outcome.cshtml", viewModel);
        //    }

        //    var viewModelStatus = new OutcomeStatusViewModel
        //    {
        //        ApplicationId = command.ApplicationId,
        //        ApplicationReferenceNumber = viewModel.ApplicationReferenceNumber,
        //        ApplicationSubmittedDate = viewModel.ApplicationSubmittedDate,
        //        OrganisationName = viewModel.OrganisationName,
        //        ProviderRoute = viewModel.ProviderRoute,
        //        Ukprn = viewModel.Ukprn,
        //        ApproveGateway = viewModel.ApproveGateway,
        //        ApproveModeration = viewModel.ApproveModeration,
        //        OversightStatus = viewModel.OversightStatus,
        //        SuccessfulText = viewModel.SuccessfulText,
        //        SuccessfulAlreadyActiveText = viewModel.SuccessfulAlreadyActiveText,
        //        SuccessfulFitnessForFundingText = viewModel.SuccessfulFitnessForFundingText,
        //        UnsuccessfulText =  viewModel.UnsuccessfulText,
        //        InProgressInternalText = viewModel.InProgressInternalText,
        //        InProgressExternalText = viewModel.InProgressExternalText,
        //        ApplicationEmailAddress = viewModel.ApplicationEmailAddress
        //    };

        //    return View("~/Views/Oversight/OutcomeHoldingPage.cshtml", viewModelStatus);

        //    //MFCMFC this all needs tidying up but will do in follow up stories
        //    // if (oversightStatus == OversightReviewStatus.Successful)
        //    // {
        //    //     return View("~/Views/Oversight/OutcomeSuccessful.cshtml", viewModelSuccess);
        //    // }
        //    //
        //    // return View("~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModelSuccess);
        //}

        //[HttpPost("Oversight/Outcome/Successful/{applicationId}")]
        //public async Task<IActionResult> Successful(Guid applicationId, string status)
        //{
        //    var oversightViewModel = await _oversightOrchestrator.GetOversightDetailsViewModel(applicationId, null);
        //    if (CheckForBackButtonAfterSubmission(oversightViewModel.OversightStatus, out var outcome)) return outcome;
        //    var errorMessages = OversightValidator.ValidateOutcomeSuccessful(status);

        //    var viewModel = new OutcomeSuccessStatusViewModel
        //    {
        //        ApplicationId = applicationId,
        //        ApplicationReferenceNumber = oversightViewModel.ApplicationReferenceNumber,
        //        ApplicationSubmittedDate = oversightViewModel.ApplicationSubmittedDate,
        //        OrganisationName = oversightViewModel.OrganisationName,
        //        ProviderRoute = oversightViewModel.ProviderRoute,
        //        Ukprn = oversightViewModel.Ukprn
        //    };

        //    if (errorMessages.Any())
        //    {
        //        viewModel.ErrorMessages = errorMessages;
        //        return View($"~/Views/Oversight/OutcomeSuccessful.cshtml", viewModel);
        //    }
        //    else if ("No".Equals(status, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        oversightViewModel.OversightStatus = OversightReviewStatus.Successful;
        //        return View($"~/Views/Oversight/Outcome.cshtml", oversightViewModel);
        //    }

        //    var userId = HttpContext.User.UserId();
        //    var userName = HttpContext.User.UserDisplayName();
        //    await _outcomeOrchestrator.RecordOutcome(applicationId, OversightReviewStatus.Successful, userId, userName);

        //    // record in database it's a success
        //    var viewModelDone = new OutcomeDoneViewModel { Ukprn = oversightViewModel.Ukprn, Status = OversightReviewStatus.Successful };

        //    return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        //}

        //[HttpPost("Oversight/Outcome/Unsuccessful/{applicationId}")]
        //public async Task<IActionResult> Unsuccessful(Guid applicationId, string status)
        //{
        //    var oversightViewModel = await _oversightOrchestrator.GetOversightDetailsViewModel(applicationId, null);
        //    if (CheckForBackButtonAfterSubmission(oversightViewModel.OversightStatus, out var outcome)) return outcome;
        //    var errorMessages = OversightValidator.ValidateOutcomeUnsuccessful(status);

        //    var viewModel = new OutcomeSuccessStatusViewModel
        //    {
        //        ApplicationId = applicationId,
        //        ApplicationReferenceNumber = oversightViewModel.ApplicationReferenceNumber,
        //        ApplicationSubmittedDate = oversightViewModel.ApplicationSubmittedDate,
        //        OrganisationName = oversightViewModel.OrganisationName,
        //        ProviderRoute = oversightViewModel.ProviderRoute,
        //        Ukprn = oversightViewModel.Ukprn
        //    };

        //    if (errorMessages.Any())
        //    {
        //        viewModel.ErrorMessages = errorMessages;
        //        return View($"~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModel);
        //    }
        //    else if ("No".Equals(status, StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        oversightViewModel.OversightStatus = OversightReviewStatus.Unsuccessful;
        //        return View($"~/Views/Oversight/Outcome.cshtml", oversightViewModel);
        //    }

        //    var userId = HttpContext.User.UserId();
        //    var userName = HttpContext.User.UserDisplayName();
        //    await _outcomeOrchestrator.RecordOutcome(applicationId, OversightReviewStatus.Unsuccessful, userId, userName);

        //    var viewModelDone = new OutcomeDoneViewModel { Ukprn = oversightViewModel.Ukprn, Status = OversightReviewStatus.Unsuccessful };

        //    return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        //}


        //private bool CheckForBackButtonAfterSubmission(string oversightStatus, out IActionResult outcome)
        //{
        //    outcome = null;
        //    if (oversightStatus == OversightReviewStatus.Successful || oversightStatus == OversightReviewStatus.Unsuccessful)
        //    {
        //        outcome = new RedirectToActionResult("Applications", "Oversight", null);
        //        return true;
        //    }

        //    return false;
        //}
    }
}
