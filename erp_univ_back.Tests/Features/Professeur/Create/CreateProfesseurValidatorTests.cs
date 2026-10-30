using erp_univ_back.Features.Professeur.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.Create;

public class CreateProfesseurValidatorTests
{
    private readonly CreateProfesseurValidator _validator = new();

    private CreateProfesseurRequest GetValidRequest() => new(
        Nom: "Randria",
        Prenom: "Hery",
        DateNaissance: new DateOnly(1980, 4, 15),
        LieuNaissance: "Antananarivo",
        NumeroTelephone: "+261340123456",
        Email: "randria.hery@univ.mg",
        NumeroCin: "101234567890"
    );

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validator_DoitBloquer_QuandNomEstVide(string? nomInvalide)
    {
        var request = GetValidRequest() with { Nom = nomInvalide! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandNomDepasse100Caracteres()
    {
        var request = GetValidRequest() with { Nom = new string('A', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData("Randria123")]
    [InlineData("Prof@Univ")]
    [InlineData("Jean_Luc")]
    public void Validator_DoitBloquer_QuandNomContientCaracteresInterdits(string nomInvalide)
    {
        var request = GetValidRequest() with { Nom = nomInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandPrenomDepasse100Caracteres()
    {
        var request = GetValidRequest() with { Prenom = new string('B', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Prenom);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validator_DoitBloquer_QuandLieuNaissanceEstVide(string? lieuInvalide)
    {
        var request = GetValidRequest() with { LieuNaissance = lieuInvalide! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LieuNaissance);
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("034012345")] // 9 chars
    [InlineData("+261340123456789012345")] // > 20 chars
    [InlineData("telephone123")]
    public void Validator_DoitBloquer_QuandTelephoneInvalide(string telInvalide)
    {
        var request = GetValidRequest() with { NumeroTelephone = telInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroTelephone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CIN_AVEC_LETTRES")]
    [InlineData("123456789012345678")] // 18 chars (> 17)
    public void Validator_DoitBloquer_QuandCinInvalide(string cinInvalide)
    {
        var request = GetValidRequest() with { NumeroCin = cinInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroCin);
    }

    [Theory]
    [InlineData("email-invalide")]
    [InlineData("test@")]
    [InlineData("@domaine.com")]
    public void Validator_DoitBloquer_QuandEmailInvalide(string emailInvalide)
    {
        var request = GetValidRequest() with { Email = emailInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandDateNaissanceDansLeFutur()
    {
        var request = GetValidRequest() with { DateNaissance = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateNaissance);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValideSansEmail()
    {
        var request = GetValidRequest() with { Email = null, Prenom = null };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
