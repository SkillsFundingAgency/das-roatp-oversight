using System;
using System.Collections.Generic;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.ViewModels
{
    public class OutcomeViewModel
    {
        public Guid ApplicationId { get; set; }
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public string ApplicationStatus { get; set; }
        public string EsfaAdminServicesBaseUrl { get; set; }

        public List<ValidationErrorDetail> ErrorMessages { get; set; }


    }
}
