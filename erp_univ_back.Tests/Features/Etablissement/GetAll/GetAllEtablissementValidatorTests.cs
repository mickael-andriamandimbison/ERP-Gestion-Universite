using erp_univ_back.Features.Etablissement.GetAll;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Etablissement.GetAll;

public class GetAllEtablissementValidatorTests
{
    private readonly GetAllEtablissementsValidator _validator = new();

    private GetAllEtablissementsRequest GetValidRequest() => new(
        Recherche: "Université",
        Ville: "Antananarivo",
        ChampTri: "nom",
        TriDecroissant: false,
        Page: 1,
        TaillePage: 10
    );

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Validator_DoitBloquer_QuandPageEstInferieureA1(int pageInvalide)
    {
        var request = GetValidRequest() with { Page = pageInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void Validator_DoitBloquer_QuandTaillePageHorsLimites(int tailleInvalide)
    {
        // Empêche la surcharge mémoire (DoS) via une taille de page excessive ou négative
        var request = GetValidRequest() with { TaillePage = tailleInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TaillePage);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandRechercheDepasseLongueurMax()
    {
        // Protection contre les abus de recherche (ReDoS / surcharge d'index)
        var request = GetValidRequest() with { Recherche = new string('A', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Recherche);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandVilleDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Ville = new string('A', 51) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Ville);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("id; DROP TABLE etablissements--")]
    [InlineData("champ_inconnu")]
    public void Validator_DoitBloquer_QuandChampTriEstNonAutorise(string champTriInvalide)
    {
        var request = GetValidRequest() with { ChampTri = champTriInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ChampTri);
    }

    [Theory]
    [InlineData("nom")]
    [InlineData("Nom")]
    [InlineData("ville")]
    [InlineData("datecreation")]
    public void Validator_DoitAccepter_QuandChampTriEstDansListeBlanche(string champTriValide)
    {
        var request = GetValidRequest() with { ChampTri = champTriValide };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ChampTri);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

