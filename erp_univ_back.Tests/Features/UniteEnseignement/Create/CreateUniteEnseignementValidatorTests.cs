namespace erp_univ_back.Tests.Features.UniteEnseignement.Create;

using erp_univ_back.Features.UniteEnseignement.Create;
using FluentValidation.TestHelper;
using Xunit;

public class CreateUniteEnseignementValidatorTests
{
    private readonly CreateUniteEnseignementValidator _validator = new();

    private CreateUniteEnseignementRequest GetValidRequest() => new(
        Nom: "Algorithmique et Structures de Données",
        Credit: 6,
        Semestre: 1,
        FiliereNiveauId: Guid.NewGuid(),
        Code: null
    );

    [Fact]
    public void Validator_DoitAccepter_QuandRequeteEstValide()
    {
        var request = GetValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validator_DoitBloquer_QuandNomEstVide(string? nom)
    {
        var request = GetValidRequest() with { Nom = nom! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandNomDepasse150Caracteres()
    {
        var request = GetValidRequest() with { Nom = new string('A', 151) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Nom);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validator_DoitBloquer_QuandCreditEstInferieurOuEgalAZero(int credit)
    {
        var request = GetValidRequest() with { Credit = credit };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Credit);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandCreditDepasse60()
    {
        var request = GetValidRequest() with { Credit = 61 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Credit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void Validator_DoitBloquer_QuandSemestreEstInvalide(int semestre)
    {
        var request = GetValidRequest() with { Semestre = semestre };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Semestre);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Validator_DoitAccepter_QuandSemestreEst1Ou2(int semestre)
    {
        var request = GetValidRequest() with { Semestre = semestre };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Semestre);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandFiliereNiveauIdEstVide()
    {
        var request = GetValidRequest() with { FiliereNiveauId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FiliereNiveauId);
    }

    [Fact]
    public void Validator_DoitBloquer_QuandCodeDepasse50Caracteres()
    {
        var request = GetValidRequest() with { Code = new string('C', 51) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}

