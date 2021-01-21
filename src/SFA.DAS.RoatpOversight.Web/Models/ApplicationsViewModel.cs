﻿using System.Collections.Generic;
using SFA.DAS.RoatpOversight.Domain;

namespace SFA.DAS.RoatpOversight.Web.Models
{
    public class ApplicationsViewModel
    {
        public PendingOversightReviews ApplicationDetails { get; set; }
        public int ApplicationCount { get; set; }
        public CompletedOversightReviews OverallOutcomeDetails { get; set; }
        public int OverallOutcomeCount { get; set; }

    }
}
