using System;

namespace SFA.DAS.RoatpOversight.Web.Models.Partials
{
    public class InProgressDetailsViewModel
    {
        public DateTime ApplicationDeterminedDate { get; set; }
        public string UserName { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
    }
}
