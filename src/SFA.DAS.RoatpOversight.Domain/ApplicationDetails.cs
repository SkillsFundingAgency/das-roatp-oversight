﻿using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class ApplicationDetails
    {
        public Guid ApplicationId { get; set; }
        public string OrganisationName { get; set; }
        public string Ukprn { get; set; }
        public string ProviderRoute { get; set; }
        public string ApplicationReferenceNumber { get; set; }
        public DateTime ApplicationSubmittedDate { get; set; }
        public string OversightStatus { get; set; }
        public string ApplicationStatus { get; set; }
        public string GatewayReviewStatus { get; set; }
        public string ModerationStatus { get; set; }
        public string FinancialReviewStatus { get; set; }
    }
}
