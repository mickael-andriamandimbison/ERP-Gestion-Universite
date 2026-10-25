using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etablissement.Update;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Etablissement.Update;

public class UpdateEtablissementValidatorTests
{
    private readonly UpdateEtablissementValidator _validator = new();

    private UpdateEtablissementRequest GetValidRequest() => new(
        Nom: "Université d'Antananarivo Modifiée",
        Adresse: "Campus Universitaire Ambohitsaina",
        CodePostal: "101",
        Ville: "Antananarivo",
        Pays: "Madagascar",
        Telephone: "+261340000001",
        Email: "update@univ-antananarivo.mg",
        SiteWeb: "https://www.univ-antananarivo.mg",
        Logo: "nouveau_logo.png",
        Sigle: "UAM",
        Description: "Description mise à jour.",
        Statut: StatusEtablissement.Actif
    );

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validator_DoitBloquer_QuandChampsObligatoiresSontVides(string? valeurVide)
    {
        var request = GetValidRequest() with
        {
            Nom = valeurVide!,
            Adresse = valeurVide!,
            CodePostal = valeurVide!,
            Ville = valeurVide!,
            Pays = valeurVide!,
            Telephone = valeurVide!,
            Email = valeurVide!
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Nom);
        result.ShouldHaveValidationErrorFor(x => x.Adresse);
        result.ShouldHaveValidationErrorFor(x => x.CodePostal);
        result.ShouldHaveValidationErrorFor(x => x.Ville);
        result.ShouldHaveValidationErrorFor(x => x.Pays);
        result.ShouldHaveValidationErrorFor(x => x.Telephone);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandChampsDepassentLongueurMax()
    {
        var request = GetValidRequest() with
        {
            Nom = new string('A', 101),
            Adresse = new string('A', 201),
            CodePostal = new string('A', 11),
            Ville = new string('A', 51),
            Pays = new string('A', 51),
            Telephone = new string('1', 21),
            Email = new string('A', 100) + "@test.com",
            SiteWeb = new string('A', 101),
            Logo = new string('A', 201),
            Sigle = new string('A', 31),
            Description = new string('A', 501)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Nom);
        result.ShouldHaveValidationErrorFor(x => x.Adresse);
        result.ShouldHaveValidationErrorFor(x => x.CodePostal);
        result.ShouldHaveValidationErrorFor(x => x.Ville);
        result.ShouldHaveValidationErrorFor(x => x.Pays);
        result.ShouldHaveValidationErrorFor(x => x.Telephone);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.SiteWeb);
        result.ShouldHaveValidationErrorFor(x => x.Logo);
        result.ShouldHaveValidationErrorFor(x => x.Sigle);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("03412A4567")]
    [InlineData("+26134 12345")]
    [InlineData("++261341234567")]
    public void Validator_DoitBloquer_QuandTelephoneInvalide(string fauxTelephone)
    {
        var request = GetValidRequest() with { Telephone = fauxTelephone };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Telephone);
    }

    [Theory]
    [InlineData("emailSansArobase")]
    [InlineData("email@domaine")]
    [InlineData("@gmail.com")]
    public void Validator_DoitBloquer_QuandEmailInvalide(string fauxEmail)
    {
        var request = GetValidRequest() with { Email = fauxEmail };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandStatutInvalide()
    {
        var request = GetValidRequest() with { Statut = (StatusEtablissement)999 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Statut);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandToutEstCorrect()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

