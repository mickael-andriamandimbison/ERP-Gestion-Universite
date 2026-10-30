using erp_univ_back.Features.AnneeAcademique.Create;
using FluentValidation.TestHelper;
using Xunit;

namespace erp_univ_back.Tests.Features.AnneeAcademique.Create;

public class CreateAnneeAcademiqueValidatorTests
{
    private readonly CreateAnneeAcademiqueValidator _validator = new();

    private CreateAnneeAcademiqueRequest GetValidRequest() => new(
        Libelle: "2025-2026",
        DateDebut: new DateOnly(2025, 9, 1),
        DateFin: new DateOnly(2026, 6, 30),
        EstActive: false
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
        var request = GetValidRequest() with { Libelle = "2025-2026-TROP-LONG" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Theory]
    [InlineData("2025")]
    [InlineData("25-26")]
    [InlineData("annee-2025")]
    [InlineData("2025_2026")]
    public void Validator_DoitBloquer_QuandFormatLibelleInvalide(string formatInvalide)
    {
        var request = GetValidRequest() with { Libelle = formatInvalide };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeFinNonConsecutive()
    {
        // 2025-2027 : l'année de fin n'est pas N+1
        var request = GetValidRequest() with
        {
            Libelle = "2025-2027",
            DateDebut = new DateOnly(2025, 9, 1),
            DateFin = new DateOnly(2027, 6, 30)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Libelle);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandDateFinAnterieureOuEgaleADateDebut()
    {
        var request = GetValidRequest() with
        {
            DateDebut = new DateOnly(2025, 9, 1),
            DateFin = new DateOnly(2025, 9, 1)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateFin);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeDateDebutNeCorrespondPasAuLibelle()
    {
        var request = GetValidRequest() with
        {
            DateDebut = new DateOnly(2024, 9, 1) // 2024 au lieu de 2025
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateDebut);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandAnneeDateFinNeCorrespondPasAuLibelle()
    {
        var request = GetValidRequest() with
        {
            DateFin = new DateOnly(2027, 6, 30) // 2027 au lieu de 2026
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DateFin);
    }

    [Theory]
    [InlineData("2025-2026")]
    [InlineData("2025/2026")]
    public void Validator_DoitAccepter_QuandRequeteEstValide(string libelleValide)
    {
        var request = GetValidRequest() with { Libelle = libelleValide };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

