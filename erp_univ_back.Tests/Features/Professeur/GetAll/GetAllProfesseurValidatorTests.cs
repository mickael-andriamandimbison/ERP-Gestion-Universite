using erp_univ_back.Features.Professeur.GetAll;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.GetAll;

public class GetAllProfesseurValidatorTests
{
    private readonly GetAllProfesseurValidator _validator = new();

    private GetAllProfesseurRequest GetValidRequest() => new(
        Recherche: "Ramaroson",
        ChampTri: "nom",
        TriDecroissant: false,
        Page: 1,
        TaillePage: 10
    );

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validator_DoitBloquer_QuandPageInferieureA1(int pageInvalide)
    {
        var request = GetValidRequest() with { Page = pageInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    [InlineData(500)]
    public void Validator_DoitBloquer_QuandTaillePageHorsLimites(int tailleInvalide)
    {
        var request = GetValidRequest() with { TaillePage = tailleInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TaillePage);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandRechercheDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Recherche = new string('A', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Recherche);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("cin; DROP TABLE professeurs--")]
    public void Validator_DoitBloquer_QuandChampTriNonAutorise(string champInvalide)
    {
        var request = GetValidRequest() with { ChampTri = champInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ChampTri);
    }

    [Theory]
    [InlineData("nom")]
    [InlineData("prenom")]
    [InlineData("matricule")]
    [InlineData("email")]
    [InlineData("datecreation")]
    public void Validator_DoitAccepter_QuandChampTriEstValide(string champValide)
    {
        var request = GetValidRequest() with { ChampTri = champValide };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_DoitAccepter_QuandChampTriEstNullOuVide()
    {
        var request = GetValidRequest() with { ChampTri = "" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

