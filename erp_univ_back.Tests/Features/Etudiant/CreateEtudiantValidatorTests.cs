using Xunit;
using FluentValidation.TestHelper;
using System;
using erp_univ_back.Features.Etudiant.Create;

namespace erp_univ_back.Tests.Features.Etudiant.Create;

public class CreateEtudiantRequestValidatorTests
{
    private readonly CreateEtudiantRequestValidator _validator = new();

    private CreateEtudiantRequest GetValidRequest() => new(
        Nom: "Rakoto",
        Prenom: "Jean",
        DateNaissance: new DateOnly(2005, 5, 12),
        LieuNaissance: "Antananarivo",
        NumeroTelephone: "+261341234567",
        Email: "jean.rakoto@univ.mg",
        Parents: null
    );

    [Fact]
    public void Validator_DoitBloquer_QuandNomEstVide()
    {
        var request = GetValidRequest() with { Nom = "" };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData("Rakot0")]
    [InlineData("Jean 😊")]
    [InlineData("Andria<script>")]
    [InlineData("Rakoto_Nirina")]
    public void Validator_DoitBloquer_QuandNomContientCaracteresInvalides(string fauxNom)
    {
        var request = GetValidRequest() with { Nom = fauxNom };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData("N'Golo")]
    [InlineData("Jean-Baptiste")]
    [InlineData("Andriamanjato")]
    [InlineData("Émeline Rasoa")]
    public void Validator_DoitAccepter_QuandNomEstValide(string vraiNom)
    {
        var request = GetValidRequest() with { Nom = vraiNom };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData("03412A4567")]
    [InlineData("+26134 12345")]
    [InlineData("034-12-345-67")]
    [InlineData("++261341234567")]
    [InlineData("034123456+")]
    [InlineData("03412")]
    public void Validator_DoitBloquer_QuandTelephoneInvalide(string fauxTelephone)
    {
        var request = GetValidRequest() with { NumeroTelephone = fauxTelephone };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroTelephone);
    }

    [Theory]
    [InlineData("0341234567")]
    [InlineData("+261341234567")]
    public void Validator_DoitAccepter_QuandTelephoneEstValide(string vraiTelephone)
    {
        var request = GetValidRequest() with { NumeroTelephone = vraiTelephone };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NumeroTelephone);
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
    public void Validator_DoitAccepter_QuandToutEstCorrect()
    {
        var request = GetValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}