using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class ApplicationDetails
    {
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
    }
}
