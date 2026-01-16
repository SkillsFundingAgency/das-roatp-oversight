using FluentValidation;

namespace SFA.DAS.RoatpOversight.Web.Validators;

public class SearchTermValidator : AbstractValidator<string>
{
    private const int MinimumSearchTermLength = 3;
    private readonly string SearchTermMandatory = "Enter an organisation name or UKPRN";
    private readonly string SearchTermLength = $"Enter a UKPRN or an organisation name using {MinimumSearchTermLength} or more characters";

    public SearchTermValidator()
    {
        RuleFor(searchTerm => searchTerm)
            .NotEmpty()
            .WithMessage(SearchTermMandatory)
            .MinimumLength(MinimumSearchTermLength)
            .WithMessage(SearchTermLength)
            .OverridePropertyName("SearchTerm");
    }
}
