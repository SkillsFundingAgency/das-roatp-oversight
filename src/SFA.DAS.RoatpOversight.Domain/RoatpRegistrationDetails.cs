﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.RoatpOversight.Domain
{
    public class RoatpRegistrationDetails
    {
        public string UKPRN { get; set; }
        public int ProviderTypeId { get; set; }
        public int OrganisationTypeId { get; set; }
        public string LegalName { get; set; }
        public string TradingName { get; set; }
        public string CharityNumber { get; set; }
        public string CompanyNumber { get; set; }
    }
}
