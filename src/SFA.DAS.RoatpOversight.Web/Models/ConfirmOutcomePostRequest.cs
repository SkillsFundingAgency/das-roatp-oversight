﻿using System;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ConfirmOutcomePostRequest
    {
        public Guid ApplicationId { get; set; }
        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; }
        public OversightReviewStatus OversightStatus { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
        public Guid OutcomeKey { get; set; }
        public string Confirm { get; set; }
    }
}
