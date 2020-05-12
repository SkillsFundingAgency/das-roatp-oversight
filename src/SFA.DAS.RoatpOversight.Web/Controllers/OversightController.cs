using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.Services;
using SFA.DAS.RoatpOversight.Web.Settings;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    //[Authorize]
    public class OversightController: Controller
    {
        private readonly IOversightOrchestrator _orchestrator;
        private readonly ILogger<OversightController> _logger;

        public OversightController(IOversightOrchestrator orchestrator, ILogger<OversightController> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        public async Task<IActionResult> Applications()
        {
            var viewModel = await _orchestrator.GetOversightOverviewViewModel();

            return View(viewModel);
        }

        [HttpGet("Oversight/Outcome/{applicationId}")]
        public IActionResult Outcome(Guid applicationId)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault();
            var vm = new OutcomeViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber =  applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate =  applicationDetails.ApplicationSubmittedDate,
                OrganisationName =  "THIS IS A DUMMY PAGE",
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn =  applicationDetails.Ukprn
            };


            return View(vm);
        }

        [HttpPost("Oversight/Outcome/{applicationId}")]
        public IActionResult EvaluateOutcome(Guid applicationId, string status)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault();
            var viewModel = new OutcomeViewModel
            {
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = "THIS IS A DUMMY PAGE",
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn = applicationDetails.Ukprn
            };

            viewModel.ErrorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(status))
            {
                viewModel.ErrorMessages.Add(new ValidationErrorDetail
                    {ErrorMessage = "error message", ValidationStatusCode = new ValidationStatusCode()});

                return View($"~/Views/Oversight/Outcome.cshtml", viewModel);
            }

            if (status.ToLower() == "successful")
            {
                var viewModelSuccessful = new OutcomeSuccessfulViewModel
                {
                    ApplicationId =  applicationDetails.ApplicationId,
                    ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                    OrganisationName = "THIS IS A DUMMY PAGE",
                    ProviderRoute = applicationDetails.ProviderRoute,
                    Ukprn = applicationDetails.Ukprn
                };
                return View("~/Views/Oversight/OutcomeSuccessful.cshtml",viewModelSuccessful);
            }

            var viewModelUnsuccessful = new OutcomeUnsuccessfulViewModel
            {
                ApplicationId =  applicationDetails.ApplicationId,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = "THIS IS A DUMMY RECORD",
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn = applicationDetails.Ukprn
            };
            return View("~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModelUnsuccessful);
        }



        [HttpPost("Oversight/Outcome/Successful/{applicationId}")]
        public IActionResult Successful(Guid applicationId, string status)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault(x => x.ApplicationId == applicationId);
            var viewModel = new OutcomeSuccessfulViewModel
            {
                ApplicationId =  applicationId,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = "THIS IS A DUMMY PAGE",
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn = applicationDetails.Ukprn
            };

            viewModel.ErrorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(status))
            {
                viewModel.ErrorMessages.Add(new ValidationErrorDetail
                { ErrorMessage = "error message", ValidationStatusCode = new ValidationStatusCode() });

                return View($"~/Views/Oversight/OutcomeSuccessful.cshtml", viewModel);
            }

            if (status.ToLower()=="no")
            {
                var viewModelOutcome = new OutcomeViewModel
                {
                    ApplicationId =  applicationId,
                    ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                    OrganisationName = "THIS IS A DUMMY PAGE",
                    ProviderRoute = applicationDetails.ProviderRoute,
                    Ukprn = applicationDetails.Ukprn,
                    ApplicationStatus = "Successful"
                };

                return View($"~/Views/Oversight/Outcome.cshtml", viewModelOutcome);
            }


            // record in database it's a success
            var viewModelDone = new OutcomeDoneViewModel { Ukprn = applicationDetails.Ukprn, Status = "Successful" };

            return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        }

        [HttpPost("Oversight/Outcome/Unsuccessful/{applicationId}")]
        public IActionResult Unsuccessful(Guid applicationId, string status)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault();
            var viewModel = new OutcomeUnsuccessfulViewModel
            {
                ApplicationId = applicationId,
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = "THIS IS A DUMMY PAGE",
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn = applicationDetails.Ukprn
            };

            viewModel.ErrorMessages = new List<ValidationErrorDetail>();
            if (string.IsNullOrEmpty(status))
            {
                viewModel.ErrorMessages.Add(new ValidationErrorDetail
                    { ErrorMessage = "error message", ValidationStatusCode = new ValidationStatusCode() });

                return View($"~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModel);
            }

            if (status.ToLower() == "no")
            {
                var viewModelOutcome = new OutcomeViewModel
                {
                    ApplicationId = applicationId,
                    ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                    OrganisationName = "THIS IS A DUMMY PAGE",
                    ProviderRoute = applicationDetails.ProviderRoute,
                    Ukprn = applicationDetails.Ukprn,
                    ApplicationStatus = "Unsuccessful"
                };

                return View($"~/Views/Oversight/Outcome.cshtml", viewModelOutcome);
            }

            // record value in database

            var viewModelDone = new OutcomeDoneViewModel {Ukprn = applicationDetails.Ukprn, Status = "Unsuccessful"};

                return View("~/Views/Oversight/OutcomeDone.cshtml",viewModelDone);
        }


        private  OverallOutcomeViewModel GetStubbedViewModel()
        {
            var viewModel = new OverallOutcomeViewModel();

            // dummy data that enables Greg to assess
            var applicationDetails = new List<ApplicationDetails>
            {
                new ApplicationDetails
                {
                    ApplicationId = new Guid("2e8ffe21-f622-4eef-af93-22e0ad0c6737"),
                    OrganisationName = "ZZZ Limited",
                    Ukprn = "123456768",
                    ProviderRoute = "Main",
                    ApplicationReferenceNumber = "APR000175",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                },
                new ApplicationDetails
                {
                    ApplicationId = new Guid("a0fb2cdc-edf1-457c-96d7-2dc69cd5d8e8"),
                    OrganisationName = "AAA Limited",
                    Ukprn = "223456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000179",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20)
                },
                new ApplicationDetails
                {
                    ApplicationId = new Guid("cb84760b-931b-4724-a7fc-81e68659da10"),
                    OrganisationName = "BBB BBB Limited",
                    Ukprn = "523456765",
                    ProviderRoute = "Supporting",
                    ApplicationReferenceNumber = "APR000173",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                }
            };

            viewModel.ApplicationDetails = applicationDetails;
            viewModel.ApplicationCount = 3;

            var outcomes = new List<OverallOutcomeDetails>
            {
                new OverallOutcomeDetails
                {
                    OrganisationName = "FFF Limited",
                    Ukprn = "443456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000132",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 01),
                    OversightStatus = "SUCCESSFUL"
                },
                new OverallOutcomeDetails
                {
                    OrganisationName = "DDD Limited",
                    Ukprn = "43234565",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000120",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 29),
                    OversightStatus = "UNSUCCESSFUL"
                }
            };

            viewModel.OverallOutcomeDetails = outcomes;
            viewModel.OverallOutcomeCount = 2;

            return viewModel;
        }
    }
}
