﻿using System;

namespace SFA.DAS.RoatpOversight.Web.Infrastructure
{
    public class EvaluationOutcomeCommand
    {
        public Guid ApplicationId { get; set; }
        public string OversightStatus { get; set; }
        public string ApproveGateway { get; set; }
        public string ApproveModeration { get; set; } 
        public string SuccessfulText { get; set; }
        public string SuccessfulAlreadyActiveText { get; set; }
        public string SuccessfulFitnessForFundingText { get; set; }
        public string UnsuccessfulText { get; set; }
        public string InProgressInternalText { get; set; }
        public string InProgressExternalText { get; set; }
        public string ApplicationEmailAddress { get; set; }
    }
}
