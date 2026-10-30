using erp_univ_back.Features.AnneeAcademique.GetAll;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.AnneeAcademique.GetAll;

public class GetAllAnneeAcademiqueValidatorTests
{
    private readonly GetAllAnneeAcademiqueValidator _validator = new();

    private GetAllAnneeAcademiqueRequest GetValidRequest() => new(
        Recherche: "2025",
        EstActive: true
    );

    [Fact]
    public void Validator_DoitBloquer_QuandRechercheDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Recherche = new string('A', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Recherche);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

