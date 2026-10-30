using erp_univ_back.Features.Etudiant.Update;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Etudiant.Update;

public class UpdateEtudiantValidatorTests
{
    private readonly UpdateEtudiantValidator _validator = new();

    private UpdateEtudiantRequest GetValidRequest() => new(
        Nom: "Randria",
        Prenom: "Michel",
        DateNaissance: new DateOnly(2004, 3, 20),
        LieuNaissance: "Fianarantsoa",
        NumeroCin: "101202303404",
        NumeroTelephone: "+261320011223",
        Email: "michel.randria@univ.mg"
    );

    [Fact]
    public void Validator_DoitBloquer_QuandNomEstVide()
    {
        var request = GetValidRequest() with { Nom = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData("Randria123")]
    [InlineData("Michel 🚀")]
    [InlineData("Randria<tag>")]
    public void Validator_DoitBloquer_QuandNomContientCaracteresInvalides(string fauxNom)
    {
        var request = GetValidRequest() with { Nom = fauxNom };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData("0320011A23")]
    [InlineData("+26132 00112")]
    [InlineData("032")]
    [InlineData("++261320011223")]
    public void Validator_DoitBloquer_QuandTelephoneInvalide(string fauxTelephone)
    {
        var request = GetValidRequest() with { NumeroTelephone = fauxTelephone };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroTelephone);
    }

    [Theory]
    [InlineData("emailSansArobase")]
    [InlineData("email@.com")]
    [InlineData("@gmail.com")]
    public void Validator_DoitBloquer_QuandEmailInvalide(string fauxEmail)
    {
        var request = GetValidRequest() with { Email = fauxEmail };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandLieuNaissanceEstVide()
    {
        var request = GetValidRequest() with { LieuNaissance = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LieuNaissance);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

