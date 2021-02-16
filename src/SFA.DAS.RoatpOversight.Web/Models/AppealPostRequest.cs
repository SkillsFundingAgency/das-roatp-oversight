using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealPostRequest
    {
        public Guid ApplicationId { get; set; }
        public string Message { get; set; }
    }
}
