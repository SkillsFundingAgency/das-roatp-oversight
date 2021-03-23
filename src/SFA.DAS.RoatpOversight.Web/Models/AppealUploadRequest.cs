using System;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealUploadRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid AppealId { get; set; }
        public Guid AppealUploadId { get; set; }
    }
}
