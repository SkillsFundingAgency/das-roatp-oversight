﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.RoatpOversight.Domain;
using SFA.DAS.RoatpOversight.Web.ViewModels;

namespace SFA.DAS.RoatpOversight.Web.Controllers
{
    //[Authorize]
    public class OversightController: Controller
    {
        public IActionResult Applications(string flag)
        {
            var viewModel = GetStubbedViewModel();

            if (flag == "no-applications" || flag=="no-details")
            {
                viewModel.ApplicationDetails = new List<ApplicationDetails>();
                viewModel.ApplicationCount = 0;
            }

            if (flag == "no-outcomes" || flag == "no-details")
            {
                viewModel.OverallOutcomeDetails = new List<OverallOutcomeDetails>();
                viewModel.OverallOutcomeCount = 0;
            }

            return View(viewModel);
        }

        [HttpGet("Oversight/Outcome/{ukprn}")]
        public IActionResult Outcome(string ukprn)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault(x => x.Ukprn == ukprn);
            var vm = new OutcomeViewModel
            {
                ApplicationReferenceNumber =  applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate =  applicationDetails.ApplicationSubmittedDate,
                OrganisationName =  applicationDetails.OrganisationName,
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn =  applicationDetails.Ukprn
            };


            return View(vm);
        }

        [HttpPost("Oversight/Outcome/{ukprn}")]
        public IActionResult EvaluateOutcome(string ukprn, string status)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault(x => x.Ukprn == ukprn);
            var viewModel = new OutcomeViewModel
            {
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
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
                    ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                    OrganisationName = applicationDetails.OrganisationName,
                    ProviderRoute = applicationDetails.ProviderRoute,
                    Ukprn = applicationDetails.Ukprn
                };
                return View("~/Views/Oversight/OutcomeSuccessful.cshtml",viewModelSuccessful);
            }

            var viewModelUnsuccessful = new OutcomeUnsuccessfulViewModel
            {
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
                ProviderRoute = applicationDetails.ProviderRoute,
                Ukprn = applicationDetails.Ukprn
            };
            return View("~/Views/Oversight/OutcomeUnsuccessful.cshtml", viewModelUnsuccessful);
        }



        [HttpPost("Oversight/Outcome/Successful/{ukprn}")]
        public IActionResult Successful(string ukprn, string status)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault(x => x.Ukprn == ukprn);
            var viewModel = new OutcomeSuccessfulViewModel
            {
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
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
                    ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                    OrganisationName = applicationDetails.OrganisationName,
                    ProviderRoute = applicationDetails.ProviderRoute,
                    Ukprn = applicationDetails.Ukprn,
                    ApplicationStatus = "Successful"
                };

                return View($"~/Views/Oversight/Outcome.cshtml", viewModelOutcome);
            }


            // record in database it's a success
            var viewModelDone = new OutcomeDoneViewModel { Ukprn = ukprn, Status = "Successful" };

            return View("~/Views/Oversight/OutcomeDone.cshtml", viewModelDone);
        }

        [HttpPost("Oversight/Outcome/Unsuccessful/{ukprn}")]
        public IActionResult Unsuccessful(string ukprn, string status)
        {
            var stubbedViewModel = GetStubbedViewModel();
            var applicationDetails = stubbedViewModel.ApplicationDetails.FirstOrDefault(x => x.Ukprn == ukprn);
            var viewModel = new OutcomeUnsuccessfulViewModel
            {
                ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                OrganisationName = applicationDetails.OrganisationName,
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
                    ApplicationReferenceNumber = applicationDetails.ApplicationReferenceNumber,
                    ApplicationSubmittedDate = applicationDetails.ApplicationSubmittedDate,
                    OrganisationName = applicationDetails.OrganisationName,
                    ProviderRoute = applicationDetails.ProviderRoute,
                    Ukprn = applicationDetails.Ukprn,
                    ApplicationStatus = "Unsuccessful"
                };

                return View($"~/Views/Oversight/Outcome.cshtml", viewModelOutcome);
            }

            // record value in database

            var viewModelDone = new OutcomeDoneViewModel {Ukprn = ukprn, Status = "Unsuccessful"};

                return View("~/Views/Oversight/OutcomeDone.cshtml",viewModelDone);
        }


        private static OverallOutcomeViewModel GetStubbedViewModel()
        {
            var viewModel = new OverallOutcomeViewModel();

            // dummy data that enables Greg to assess
            var applicationDetails = new List<ApplicationDetails>
            {
                new ApplicationDetails
                {
                    OrganisationName = "ZZZ Limited",
                    Ukprn = "123456768",
                    ProviderRoute = "Main",
                    ApplicationReferenceNumber = "APR000175",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 21)
                },
                new ApplicationDetails
                {
                    OrganisationName = "AAA Limited",
                    Ukprn = "223456768",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000179",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20)
                },
                new ApplicationDetails
                {
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
                    Outcome = "SUCCESSFUL"
                },
                new OverallOutcomeDetails
                {
                    OrganisationName = "DDD Limited",
                    Ukprn = "43234565",
                    ProviderRoute = "Employer",
                    ApplicationReferenceNumber = "APR000120",
                    ApplicationSubmittedDate = new DateTime(2019, 10, 20),
                    ApplicationDeterminedDate = new DateTime(2019, 10, 29),
                    Outcome = "UNSUCCESSFUL"
                }
            };

            viewModel.OverallOutcomeDetails = outcomes;
            viewModel.OverallOutcomeCount = 2;

            return viewModel;
        }
    }
}
