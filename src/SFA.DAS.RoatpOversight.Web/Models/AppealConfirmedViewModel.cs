using System;
using System.Linq;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class AppealConfirmedViewModel
    {
        public Guid ApplicationId { get; set; }
        public string AppealStatus { get; set; }

        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }

        public string Status =>
            AppealStatuses.SuccessfulStatuses.Contains(AppealStatus)
                ? SFA.DAS.RoatpOversight.Domain.AppealStatus.Successful
                : AppealStatus;
    }
}