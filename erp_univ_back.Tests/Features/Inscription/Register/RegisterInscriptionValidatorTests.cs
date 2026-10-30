namespace erp_univ_back.Tests.Features.Inscription.Register;

using erp_univ_back.Features.Etudiant.Create;
using erp_univ_back.Features.Inscription.Register;
using FluentValidation.TestHelper;
using Xunit;

public class RegisterInscriptionValidatorTests
{
    private readonly RegisterInscriptionValidator _validator = new();

    private CreateEtudiantRequest GetValidEtudiantRequest() => new(
        Nom: "Randria",
        Prenom: "Hery",
        DateNaissance: new DateOnly(2003, 5, 20),
        LieuNaissance: "Antananarivo",
        NumeroTelephone: "+261340123456",
        Email: "randria.hery@univ.mg",
        NumeroCin: "101234567890",
        Parents: []
    );

    private RegisterInscriptionRequest GetValidRequest() => new(
        EtudiantInfos: GetValidEtudiantRequest(),
        IdFiliereNiveau: Guid.NewGuid(),
        IdAnneeAcademique: Guid.NewGuid()
    );

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_DoitBloquer_QuandEtudiantInfosEstNull()
    {
        var request = GetValidRequest() with { EtudiantInfos = null! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EtudiantInfos);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandIdFiliereNiveauEstVide()
    {
        var request = GetValidRequest() with { IdFiliereNiveau = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.IdFiliereNiveau);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandIdAnneeAcademiqueEstVide()
    {
        var request = GetValidRequest() with { IdAnneeAcademique = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.IdAnneeAcademique);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validator_DoitBloquer_QuandNomEtudiantEstInvalide(string? nomInvalide)
    {
        var etudiant = GetValidEtudiantRequest() with { Nom = nomInvalide! };
        var request = GetValidRequest() with { EtudiantInfos = etudiant };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("EtudiantInfos.Nom");
    }

    [Theory]
    [InlineData("")]
    [InlineData("email-invalide")]
    [InlineData("test@")]
    public void Validator_DoitBloquer_QuandEmailEtudiantEstInvalide(string emailInvalide)
    {
        var etudiant = GetValidEtudiantRequest() with { Email = emailInvalide };
        var request = GetValidRequest() with { EtudiantInfos = etudiant };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("EtudiantInfos.Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("telephone-invalide")]
    public void Validator_DoitBloquer_QuandTelephoneEtudiantEstInvalide(string telInvalide)
    {
        var etudiant = GetValidEtudiantRequest() with { NumeroTelephone = telInvalide };
        var request = GetValidRequest() with { EtudiantInfos = etudiant };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("EtudiantInfos.NumeroTelephone");
    }
}

