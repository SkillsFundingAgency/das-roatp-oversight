using System;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class UpdateOrganisationApplicationDeterminedDateRequest
    {
        public DateTime ApplicationDeterminedDate { get; set; }

        public Guid OrganisationId { get; set; }
        public string LegalName { get; set; }
        public string UpdatedBy { get; set; }
    }
}
