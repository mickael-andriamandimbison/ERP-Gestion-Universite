using FluentValidation.TestHelper;
using erp_univ_back.Features.Etudiant.Create;
using Xunit;

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
        NumeroCin: "101234567890",
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

    [Theory]
    [InlineData("12345")] // Trop court (5 chiffres)
    [InlineData("10123456789")] // 11 chiffres
    [InlineData("1012345678901")] // 13 chiffres
    [InlineData("10123456789012345")] // 17 chiffres
    public void Validator_DoitBloquer_QuandCinLongueurDifferenteDe12(string fauxCin)
    {
        var request = GetValidRequest() with { NumeroCin = fauxCin };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroCin);
    }

    [Theory]
    [InlineData("10123456789A")]
    [InlineData("101-234-5678")]
    [InlineData("CIN123456789")]
    [InlineData("            ")]
    public void Validator_DoitBloquer_QuandCinContientCaracteresNonNumeriques(string fauxCin)
    {
        var request = GetValidRequest() with { NumeroCin = fauxCin };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.NumeroCin);
    }

    [Theory]
    [InlineData("101234567890")]
    [InlineData("201999888777")]
    public void Validator_DoitAccepter_QuandCinEstValide(string vraiCin)
    {
        var request = GetValidRequest() with { NumeroCin = vraiCin };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.NumeroCin);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandCinEstNullOuVide()
    {
        var requestNull = GetValidRequest() with { NumeroCin = null };
        var resultNull = _validator.TestValidate(requestNull);
        resultNull.ShouldNotHaveValidationErrorFor(x => x.NumeroCin);

        var requestVide = GetValidRequest() with { NumeroCin = "" };
        var resultVide = _validator.TestValidate(requestVide);
        resultVide.ShouldNotHaveValidationErrorFor(x => x.NumeroCin);
    }

    [Fact]
    public void Validator_DoitAccepter_QuandToutEstCorrect()
    {
        var request = GetValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
