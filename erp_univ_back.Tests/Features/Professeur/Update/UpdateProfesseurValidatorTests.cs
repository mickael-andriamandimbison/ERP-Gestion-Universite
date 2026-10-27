using erp_univ_back.Features.Professeur.Update;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.Professeur.Update;

public class UpdateProfesseurValidatorTests
{
    private readonly UpdateProfesseurValidator _validator = new();

    private UpdateProfesseurRequest GetValidRequest() => new(
        Nom: "Rakotomalala",
        Prenom: "Andry",
        DateNaissance: new DateOnly(1982, 3, 10),
        LieuNaissance: "Antsirabe",
        NumeroTelephone: "+261331234567",
        Email: "andry.rakoto@univ.mg",
        NumeroCin: "111222333444"
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
    [InlineData("Prof123")]
    [InlineData("Nom#Invalide")]
    public void Validator_DoitBloquer_QuandNomContientCaracteresInterdits(string nomInvalide)
    {
        var request = GetValidRequest() with { Nom = nomInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandPrenomDepasse100Caracteres()
    {
        var request = GetValidRequest() with { Prenom = new string('P', 101) };
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
    [InlineData("03312")] // trop court
    [InlineData("+261331234567890123456")] // trop long
    [InlineData("abc-def-ghij")]
    public void Validator_DoitBloquer_QuandTelephoneInvalide(string telInvalide)
    {
        var request = GetValidRequest() with { NumeroTelephone = telInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroTelephone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("CIN_INVALIDE")]
    [InlineData("123456789012345678")] // 18 chars
    public void Validator_DoitBloquer_QuandCinInvalide(string cinInvalide)
    {
        var request = GetValidRequest() with { NumeroCin = cinInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroCin);
    }

    [Theory]
    [InlineData("format_incorrect")]
    [InlineData("invalide@")]
    public void Validator_DoitBloquer_QuandEmailInvalide(string emailInvalide)
    {
        var request = GetValidRequest() with { Email = emailInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandDateNaissanceDansLeFutur()
    {
        var request = GetValidRequest() with { DateNaissance = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)) };
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
