﻿using System;

namespace SFA.DAS.RoatpOversight.Domain.ApiTypes
{
    public class GetOversightReviewResponse
    {
        public Guid Id { get; set; }
        public OversightReviewStatus Status { get; set; }
        public DateTime? ApplicationDeterminedDate { get; set; }
        public DateTime? InProgressDate { get; set; }
        public string InProgressUserId { get; set; }
        public string InProgressUserName { get; set; }
        public string InProgressInternalComments { get; set; }
        public string InProgressExternalComments { get; set; }
        public bool? GatewayApproved { get; set; }
        public bool? ModerationApproved { get; set; }
        public string InternalComments { get; set; }
        public string ExternalComments { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
