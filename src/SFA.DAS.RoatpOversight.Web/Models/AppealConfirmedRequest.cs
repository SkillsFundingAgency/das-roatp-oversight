using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealConfirmedRequest
    {
        public Guid ApplicationId { get; set; }
        public string AppealStatus { get; set; }
    }
}