using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class UpdateOrganisationRequest
    {
        public int ProviderTypeId { get; set; }
        public int OrganisationTypeId { get; set; }
        public string LegalName { get; set; }
        public string TradingName { get; set; }
        public string CharityNumber { get; set; }
        public string CompanyNumber { get; set; }
        public string Username { get; set; }
        public DateTime? ApplicationDeterminedDate { get; set; }
        public Guid OrganisationId { get; set; }
    }
}