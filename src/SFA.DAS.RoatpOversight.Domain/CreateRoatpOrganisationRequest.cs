namespace SFA.DAS.RoatpOversight.Domain;

public class CreateRoatpOrganisationRequest
{
    public string Ukprn { get; set; }
    public string LegalName { get; set; }
    public string TradingName { get; set; }
    public string CompanyNumber { get; set; }
    public string CharityNumber { get; set; }
    public ProviderType ProviderType { get; set; }
    public int OrganisationTypeId { get; set; }
    public string RequestingUserId { get; set; }
}
