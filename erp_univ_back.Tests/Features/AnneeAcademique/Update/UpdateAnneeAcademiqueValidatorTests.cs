using erp_univ_back.Features.AnneeAcademique.Update;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.AnneeAcademique.Update;

public class UpdateAnneeAcademiqueValidatorTests
{
    private readonly UpdateAnneeAcademiqueValidator _validator = new();

    private UpdateAnneeAcademiqueRequest GetValidRequest() => new(
        Libelle: "2026-2027",
        DateDebut: new DateOnly(2026, 9, 1),
        DateFin: new DateOnly(2027, 6, 30)
    );

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validator_DoitBloquer_QuandLibelleEstVide(string? libelleVide)
    {
        var request = GetValidRequest() with { Libelle = libelleVide! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandLibelleDepasseLongueurMax()
    {
        var request = GetValidRequest() with { Libelle = "2026-2027-TROP-LONG" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Theory]
    [InlineData("2026")]
    [InlineData("26-27")]
    [InlineData("annee-2026")]
    [InlineData("2026_2027")]
    public void Validator_DoitBloquer_QuandFormatLibelleInvalide(string formatInvalide)
    {
        var request = GetValidRequest() with { Libelle = formatInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeFinNonConsecutive()
    {
        var request = GetValidRequest() with
        {
            Libelle = "2026-2028",
            DateDebut = new DateOnly(2026, 9, 1),
            DateFin = new DateOnly(2028, 6, 30)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandDateFinAnterieureOuEgaleADateDebut()
    {
        var request = GetValidRequest() with
        {
            DateDebut = new DateOnly(2026, 9, 1),
            DateFin = new DateOnly(2026, 9, 1)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateFin);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeDateDebutNeCorrespondPasAuLibelle()
    {
        var request = GetValidRequest() with
        {
            DateDebut = new DateOnly(2025, 9, 1) // 2025 au lieu de 2026
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateDebut);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeDateFinNeCorrespondPasAuLibelle()
    {
        var request = GetValidRequest() with
        {
            DateFin = new DateOnly(2028, 6, 30) // 2028 au lieu de 2027
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateFin);
    }

    [Theory]
    [InlineData("2026-2027")]
    [InlineData("2026/2027")]
    public void Validator_DoitAccepter_QuandRequeteEstValide(string libelleValide)
    {
        var request = GetValidRequest() with { Libelle = libelleValide };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

